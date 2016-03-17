using Jasily.Desktop.Management.Diagnostics;
using JryVideo.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JryVideo.Main
{
    public class ProcessTrackTask
    {
        private readonly MainViewModel viewModelSource;

        public event EventHandler<VideoInfoViewModel> CurrentWatchVideo;

        public ProcessTrackTask(MainViewModel viewModelSource)
        {
            this.viewModelSource = viewModelSource;
            var tracker = new ProcessTracker();
            tracker.Start(10000);
            tracker.ProcessStarted += this.Tracker_ProcessStarted;
            tracker.ProcessStoped += Tracker_ProcessStoped;
        }

        private static void Tracker_ProcessStoped(object sender, int e)
        {

        }

        private void Tracker_ProcessStarted(object sender, int e)
        {
            Task.Run(() =>
            {
                Process p;
                try
                {
                    p = Process.GetProcessById(e);
                }
                catch (ArgumentException)
                {
                    return;
                }
                this.Test(p);
            });
        }

        private void Test(params Process[] ps)
        {
            if (ps.Length == 0) return;
            VideoInfoViewModel[] videos = null;
            this.GetUIDispatcher().Invoke(() =>
            {
                videos = this.viewModelSource.VideosViewModel.Items.Collection.ToArray();
            });
            Debug.Assert(videos != null);
            foreach (var p in ps)
            {
                var name = GetVideoNameByProcess(p);
                if (name == null) return;
                Debug.WriteLine($"cat process main windows name: [{name}]");
                Func<string, string> func = x => x.Replace(".", "").Replace("_", "").Replace(" ", "");
                name = func(name);
                var nameWithVideo = videos.Select(z => new
                {
                    Names = z.SeriesView.Source.Names.Concat(z.Source.Names)
                        .Select(x => func(x))
                        .Where(x => x.Length > 5)
                        .ToArray(),
                    Video = z
                }).ToArray();
                var video = nameWithVideo.FirstOrDefault(z => z.Names.Any(x => name.Contains(x)));
                if (video != null)
                {
                    this.CurrentWatchVideo?.Invoke(this, video.Video);
                }
            }
        }

        private static string GetVideoNameByProcess(Process p)
        {
            if (p == null) return null;

            try
            {
                if (p.ProcessName.Contains("potplayer", StringComparison.OrdinalIgnoreCase))
                {
                    p.WaitForInputIdle();
                    Thread.Sleep(5000);
                    return p.MainWindowTitle;
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }
    }
}