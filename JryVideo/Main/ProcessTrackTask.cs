using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Jasily.Desktop.Management.Diagnostics;
using JryVideo.Common;

namespace JryVideo.Main
{
    public class ProcessTrackTask
    {
        private readonly ProcessTracker tracker;
        private readonly MainViewModel viewModelSource;
        private readonly HashSet<int> newProcessIds = new HashSet<int>();
        private readonly HashSet<int> videoProcessIds = new HashSet<int>();
        private readonly IPlayerProcessInfo[] playerProcessInfos = {
            new PotPlayerProcessInfo()
        };

        public event EventHandler<VideoInfoViewModel> CurrentWatchVideo;

        public ProcessTrackTask(MainViewModel viewModelSource)
        {
            this.viewModelSource = viewModelSource;
            this.tracker = new ProcessTracker();
            this.tracker.ProcessStarted += this.Tracker_ProcessStarted;
            this.tracker.ProcessStoped += this.Tracker_ProcessStoped;
        }

        public void Start()
        {
            this.tracker.Start(5000);
            Task.Run(async () =>
            {
                while (true)
                {
                    int[] ps;
                    lock (this.newProcessIds)
                    {
                        ps = this.newProcessIds.ToArray();
                        this.newProcessIds.Clear();
                    }
                    await this.Test(ps.Select(GetProcessById).Where(z => z != null).ToArray());
                    await Task.Delay(5000);
                }
            });
        }

        private static Process GetProcessById(int id)
        {
            try
            {
                return Process.GetProcessById(id);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private void Tracker_ProcessStoped(object sender, int e)
        {
            Task.Run(() =>
            {
                lock (this.videoProcessIds)
                {
                    this.videoProcessIds.Remove(e);
                }

                lock (this.newProcessIds)
                {
                    this.newProcessIds.Remove(e);
                }

                Debug.WriteLine($"cat dead process [{e}]");
            });
        }

        private void Tracker_ProcessStarted(object sender, int e)
        {
            Task.Run(() =>
            {
                lock (this.newProcessIds)
                {
                    this.newProcessIds.Add(e);
                }

                Debug.WriteLine($"cat new process [{e}]");
            });
        }

        private async Task Test(Process[] ps)
        {
            if (ps.Length == 0) return;

            Func<string, string> func = x => x.Replace(".", "").Replace("_", "").Replace(" ", "");
            var videos = new Lazy<VideoInfoViewModel[]>(() =>
            {
                VideoInfoViewModel[] z = null;
                this.GetUIDispatcher().Invoke(() =>
                {
                    z = this.viewModelSource.VideosViewModel.Items.Collection.ToArray();
                });
                return z;
            });
            var nameWithVideo2 = new Lazy<Tuple<string[], VideoInfoViewModel>[]>(() =>
            {
                return videos.Value.Select(z => Tuple.Create(z.SeriesView.Source.Names.Concat(z.Source.Names)
                        .Select(x => func(x))
                        .Where(x => x.Length > 5)
                        .ToArray(), z))
                    .ToArray();
            });

            foreach (var p in ps)
            {
                string name = null;
                foreach (var infocur in this.playerProcessInfos)
                {
                    name = await infocur.TryGetVideoName(p);
                    if (name != null) break;
                }

                if (name != null)
                {
                    Debug.WriteLine($"cat process main windows name: [{name}]");
                    
                    name = func(name);
                    var nameWithVideo = nameWithVideo2.Value;

                    var matchedVideos = nameWithVideo
                        .Where(z => z.Item1.Any(x => name.Contains(x, StringComparison.OrdinalIgnoreCase)))
                        .Select(z => z.Item2)
                        .ToArray();
                    var filter = this.viewModelSource.VideosViewModel.Filter;
                    if (filter != null) matchedVideos = matchedVideos.Where(filter.Where).ToArray();
                    var video = matchedVideos.FirstOrDefault();
                    if (video != null)
                    {
                        this.CurrentWatchVideo?.Invoke(this, video);
                    }
                }
            }
        }
    }

    public interface IPlayerProcessInfo
    {
        Task<string> TryGetVideoName(Process p);
    }

    public class PotPlayerProcessInfo : IPlayerProcessInfo
    {
        public async Task<string> TryGetVideoName(Process p)
        {
            string title;
            try
            {
                if (p.ProcessName.Contains("potplayer", StringComparison.OrdinalIgnoreCase))
                {
                    p.WaitForInputIdle();
                    await Task.Delay(4000);
                    title = p.MainWindowTitle;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            if (title.Length > 12 && title.EndsWith(" - potplayer", StringComparison.OrdinalIgnoreCase))
            {
                return title.Substring(0, title.Length - 12);
            }
            else
            {
                return title;
            }
        }
    }
}