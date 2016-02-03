using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public class DatabaseHealthTester
    {
        private readonly List<string> messages = new List<string>();
        private readonly DataCenter dataCenter;
        private readonly Dictionary<string, CoverRef> coverRefs = new Dictionary<string, CoverRef>();
        private readonly Dictionary<string, List<VideoInfoRef>> videoInfoRefs = new Dictionary<string, List<VideoInfoRef>>();

        public DatabaseHealthTester(DataCenter dataCenter)
        {
            this.dataCenter = dataCenter;
        }

        [Conditional("DEBUG")]
        public void RunOnDebugAsync()
        {
            //this.RunAsync(false);
        }

        public async Task RunAsync(bool fix)
        {
            await Task.Run(async () =>
            {
                await this.dataCenter.CoverManager.Source.CursorAsync(z =>
                {
                    this.coverRefs.Add(z.Id, new CoverRef(z));
                });
                await this.dataCenter.VideoRoleManager.Source.CursorAsync(z =>
                {
                    z.MajorRoles?.ForEach(this.ConnectToCover);
                    z.MinorRoles?.ForEach(this.ConnectToCover);
                });
                await this.dataCenter.SeriesManager.Source.CursorAsync(z =>
                {
                    foreach (var jryVideoInfo in z.Videos)
                    {
                        List<VideoInfoRef> videoInfoRefs;
                        if (!this.videoInfoRefs.TryGetValue(jryVideoInfo.Id, out videoInfoRefs))
                        {
                            videoInfoRefs = new List<VideoInfoRef>();
                            this.videoInfoRefs.Add(jryVideoInfo.Id, videoInfoRefs);
                        }
                        videoInfoRefs.Add(new VideoInfoRef(z, jryVideoInfo));
                    }

                    z.Videos.ForEach(this.ConnectToCover);
                    z.Videos.Select(x => x.BackgroundImageAsCoverParent()).ForEach(this.ConnectToCover);
                });

                foreach (var coverRef in this.coverRefs.Values)
                {
                    if (coverRef.RefSources.Count != 1)
                    {
                        if (coverRef.RefSources.Count > 1)
                        {
                            this.messages.Add($"cover [{coverRef.Id}] ({coverRef.CoverType}) contain more then 1 ref:");
                            foreach (var refSource in coverRef.RefSources)
                            {
                                this.messages.Add($"  {refSource.Type} [{refSource.Id}]");
                            }
                        }
                        else
                        {
                            this.messages.Add($"cover [{coverRef.Id}] ({coverRef.CoverType}) has no ref:.");
                        }
                    }
                }

                foreach (var videoInfoRef in this.videoInfoRefs)
                {
                    if (videoInfoRef.Value.Count > 1)
                    {
                        this.messages.Add($"same video id [{videoInfoRef.Key}] exist:");
                        foreach (var infoRef in videoInfoRef.Value)
                        {
                            this.messages.Add($"  series [{infoRef.SeriesId}] [{infoRef.Id}]");
                        }
                    }
                }

                var msg = this.messages.Count == 0 ? "db safe!" : this.messages.AsLines();
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(msg);
                }
                else
                {
                    Log.Write(msg);
                }

                // remove all missing source cover
                if (fix)
                {
                    foreach (var id in this.coverRefs.Values.Where(z => z.RefSources.Count == 0).Select(z => z.Id))
                    {
                        await this.dataCenter.CoverManager.RemoveAsync(id);
                    }
                }
            });
        }

        private void ConnectToCover(IJryCoverParent obj)
        {
            var coverId = obj.CoverId;
            if (coverId == null) return;
            CoverRef cover;
            if (this.coverRefs.TryGetValue(coverId, out cover))
            {
                cover.RefSources.Add(new RefSource(obj));
            }
            else
            {
                this.messages.Add($"missing cover [{coverId}]");
            }
        }

        private class ObjectRef
        {
            public ObjectRef(JryObject obj)
            {
                this.Id = obj.Id;
            }

            public string Id { get; }
        }

        private class CoverRef : ObjectRef
        {
            public CoverRef(JryCover obj)
                : base(obj)
            {
                this.RefSources = new List<RefSource>();
                this.CoverType = obj.CoverType;
            }

            public List<RefSource> RefSources { get; }

            public JryCoverType CoverType { get; }
        }

        private class VideoInfoRef : ObjectRef
        {
            public VideoInfoRef(JrySeries series, JryVideoInfo obj)
                : base(obj)
            {
                this.SeriesId = series.Id;
            }

            public string SeriesId { get; }
        }

        private class RefSource
        {
            public RefSource(IJryCoverParent obj)
            {
                this.Id = obj.Id;
                this.Type = obj.GetType().Name;
            }

            public string Type { get; }

            public string Id { get; }
        }
    }
}