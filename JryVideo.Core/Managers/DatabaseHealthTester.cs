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
        private readonly Dictionary<string, SeriesRef> seriesRefs = new Dictionary<string, SeriesRef>();

        public DatabaseHealthTester(DataCenter dataCenter)
        {
            this.dataCenter = dataCenter;
        }

        [Conditional("DEBUG")]
        public async void RunOnDebugAsync()
        {
            await this.RunAsync(false);
        }

        public async Task RunAsync(bool fix)
        {
            await Task.Run(async () =>
            {
                await this.dataCenter.CoverManager.Source.CursorAsync(this.BuildCover);
                await this.dataCenter.SeriesManager.Source.CursorAsync(z =>
                {
                    this.seriesRefs.Add(z.Id, new SeriesRef(z));
                    foreach (var jryVideoInfo in z.Videos)
                    {
                        List<VideoInfoRef> videoInfoRefs;
                        if (!this.videoInfoRefs.TryGetValue(jryVideoInfo.Id, out videoInfoRefs))
                        {
                            videoInfoRefs = new List<VideoInfoRef>();
                            this.videoInfoRefs.Add(jryVideoInfo.Id, videoInfoRefs);
                        }
                        videoInfoRefs.Add(new VideoInfoRef(z.Id, jryVideoInfo));
                    }

                    z.Videos.ForEach(this.ConnectToCover);
                    z.Videos.Select(x => x.BackgroundImageAsCoverParent()).ForEach(this.ConnectToCover);
                });
                await this.dataCenter.VideoRoleManager.Source.CursorAsync(z =>
                {
                    if (!this.videoInfoRefs.ContainsKey(z.Id) &&
                        !this.seriesRefs.ContainsKey(z.Id))
                    {
                        this.messages.Add($"role missing source [{z.Id}]");
                    }
                    z.MajorRoles?.ForEach(this.ConnectToCover);
                    z.MinorRoles?.ForEach(this.ConnectToCover);
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
                            this.messages.Add($"cover [{coverRef.Id}] ({coverRef.CoverType}) has no ref.");
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

                // remove all missing source cover
                if (fix)
                {
                    foreach (var coverRef in this.coverRefs.Values)
                    {
                        if (coverRef.RefSources.Count != 1)
                        {
                            if (coverRef.RefSources.Count < 1)
                            {
                                await this.dataCenter.CoverManager.RemoveAsync(coverRef.Id);
                            }
                            else if (coverRef.RefSources.Count > 1)
                            {

                            }
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
            });
        }

        private void BuildCover(JryCover cover)
        {
            switch (cover.CoverType)
            {
                case JryCoverType.Artist:
                    break;

                case JryCoverType.Video:
                    if (cover.VideoId == null)
                    {
                        this.AddMessage($"cover [{cover.Id}] type [{cover.CoverType}] missing video Id.");
                    }
                    break;

                case JryCoverType.Background:
                    if (cover.VideoId == null)
                    {
                        this.AddMessage($"cover [{cover.Id}] type [{cover.CoverType}] missing video Id.");
                    }
                    break;

                case JryCoverType.Role:
                    if (cover.VideoId == null && cover.SeriesId == null)
                    {
                        this.AddMessage($"cover [{cover.Id}] type [{cover.CoverType}] missing video/series Id.");
                    }
                    if (cover.ActorId == null)
                    {
                        this.AddMessage($"cover [{cover.Id}] type [{cover.CoverType}] missing actor Id.");
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.coverRefs.Add(cover.Id, new CoverRef(cover));
        }

        private void AddMessage(string msg) => this.messages.Add(msg);

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
                this.AddMessage($"missing cover [{coverId}]");
            }
        }

        private class ObjectRef
        {
            protected ObjectRef(JryObject obj)
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

        private class SeriesRef : ObjectRef
        {
            public SeriesRef(JrySeries series)
                : base(series)
            {
            }
        }

        private class VideoInfoRef : ObjectRef
        {
            public VideoInfoRef(string seriesId, JryVideoInfo obj)
                : base(obj)
            {
                this.SeriesId = seriesId;
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