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
        private readonly List<Error> Errors = new List<Error>();

        public DatabaseHealthTester(DataCenter dataCenter)
        {
            this.dataCenter = dataCenter;
        }

        [Conditional("DEBUG")]
        public async void RunOnDebugAsync() => await this.RunAsync(false);

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

                    z.Videos.ForEach(x => this.ConnectToCover(z.Id, x));
                    z.Videos.Select(x => x.BackgroundImageAsCoverParent()).ForEach(x => this.ConnectToCover(z.Id, x));
                });
                await this.dataCenter.VideoRoleManager.Source.CursorAsync(z =>
                {
                    if (!this.videoInfoRefs.ContainsKey(z.Id) &&
                        !this.seriesRefs.ContainsKey(z.Id))
                    {
                        this.Errors.Add(Error.RoleMissingSource(z.Id));
                    }
                    z.MajorRoles?.ForEach(x => this.ConnectToCover(z.Id, x));
                    z.MinorRoles?.ForEach(x => this.ConnectToCover(z.Id, x));
                });
                await this.dataCenter.ArtistManager.Source.CursorAsync(z =>
                {
                    this.ConnectToCover(z.Id, z);
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
                            this.Errors.Add(Error.CoverMissingRef(coverRef));
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

                this.Errors.Reset(this.Errors.OrderBy(z => z.Id).ToArray());
                // show message
                if (Debugger.IsAttached)
                {
                    string id = null;
                    foreach (var error in this.Errors)
                    {
                        if (error.Id != id)
                        {
                            Debug.WriteLine("");
                            id = error.Id;
                            Debug.WriteLine($"---id [{id}]");
                        }
                        Debug.Assert(id != null);
                        Debug.WriteLine(error.ToString());
                    }
                }
                else
                {
                    string id = null;
                    foreach (var error in this.Errors)
                    {
                        if (error.Id != id)
                        {
                            Log.Write("");
                            id = error.Id;
                            Log.Write($"---id [{id}]");
                        }
                        Debug.Assert(id != null);
                        Log.Write(error.ToString());
                    }
                }

                // remove all missing source cover
                if (fix)
                {
                    foreach (var error in this.Errors) await error.FixAsync(this.dataCenter);
                }

                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(this.messages.Count + this.Errors.Count == 0 ? "db safe!" : this.messages.AsLines());
                }
                else
                {
                    Log.Write(this.messages.Count + this.Errors.Count == 0 ? "db safe!" : this.messages.AsLines());
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

        private void ConnectToCover(string queryId, IJryCoverParent obj)
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
                this.Errors.Add(Error.MissingCover(obj, queryId));
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
                this.SeriesId = obj.SeriesId;
                this.VideoId = obj.VideoId;
                this.ActorId = obj.ActorId;
            }

            public List<RefSource> RefSources { get; }

            public JryCoverType CoverType { get; }

            public string SeriesId { get; }

            public string VideoId { get; }

            public string ActorId { get; }
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

        private class Error
        {
            public ErrorType Type { get; }

            public Error(ErrorType errorType)
            {
                this.Type = errorType;
            }

            public string Id { get; protected set; }

            public static Error RoleMissingSource(string roleCollectionId) => new Error(ErrorType.RoleMissingSource) { Id = roleCollectionId };

            public static Error CoverMissingRef(CoverRef cover) => new CoverError(ErrorType.CoverMissingRef, cover);

            public static Error MissingCover(IJryCoverParent owner, string queryId) => new MissingCoverError(ErrorType.MissingCover, owner, queryId);

            public override string ToString()
            {
                switch (this.Type)
                {
                    case ErrorType.RoleMissingSource:
                        return $"role missing source [{this.Id}]";

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public virtual async Task FixAsync(DataCenter dataCenter)
            {
                switch (this.Type)
                {
                    case ErrorType.RoleMissingSource:
                        await dataCenter.VideoRoleManager.RemoveAsync(this.Id);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private class MissingCoverError : Error
        {
            private readonly IJryCoverParent owner;
            private readonly string queryId;

            public MissingCoverError(ErrorType errorType, IJryCoverParent owner, string queryId)
                : base(errorType)
            {
                this.owner = owner;
                this.queryId = queryId;
                this.Id = owner.Id;
            }

            public override string ToString()
            {
                switch (this.Type)
                {
                    case ErrorType.MissingCover:
                        return $"missing cover [{this.owner.CoverId}] (type [{this.owner.CoverType}]) ext (id [{this.owner.Id}])";

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override async Task FixAsync(DataCenter dataCenter)
            {
                switch (this.Type)
                {
                    case ErrorType.MissingCover:
                        switch (this.owner.CoverType)
                        {
                            case JryCoverType.Artist:
                                break;
                            case JryCoverType.Video:
                                break;
                            case JryCoverType.Background:
                                break;
                            case JryCoverType.Role:
                                var item = await dataCenter.VideoRoleManager.FindAsync(this.queryId);
                                if (item != null)
                                {
                                    var role = item.MajorRoles?.Find(z => z.CoverId == this.owner.CoverId) ??
                                               item.MinorRoles?.Find(z => z.CoverId == this.owner.CoverId);
                                    if (role != null)
                                    {
                                        role.CoverId = null;
                                        await dataCenter.VideoRoleManager.UpdateAsync(item);
                                    }
                                }
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private class CoverError : Error
        {
            private readonly CoverRef cover;

            public CoverError(ErrorType errorType, CoverRef cover)
                : base(errorType)
            {
                this.cover = cover;
                switch (errorType)
                {
                    case ErrorType.CoverMissingRef:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null);
                }
                switch (cover.CoverType)
                {
                    case JryCoverType.Role:
                    case JryCoverType.Artist:
                        this.Id = cover.ActorId;
                        this.Id = cover.ActorId;
                        break;

                    case JryCoverType.Video:
                    case JryCoverType.Background:
                        this.Id = cover.VideoId;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override async Task FixAsync(DataCenter dataCenter)
            {
                switch (this.Type)
                {
                    case ErrorType.CoverMissingRef:
                        await dataCenter.CoverManager.RemoveAsync(this.cover.Id);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override string ToString()
            {
                switch (this.Type)
                {
                    case ErrorType.CoverMissingRef:
                        switch (this.cover.CoverType)
                        {
                            case JryCoverType.Artist:
                                break;

                            case JryCoverType.Video:
                                break;

                            case JryCoverType.Background:
                                return $"cover [{this.cover.Id}] has no ref (type [{this.cover.CoverType}]) ext (video [{this.cover.VideoId}]).";

                            case JryCoverType.Role:
                                if (this.cover.SeriesId != null)
                                    return $"cover [{this.cover.Id}] has no ref (type [{this.cover.CoverType}]) ext (series [{this.cover.SeriesId}], actor [{this.cover.ActorId}]).";
                                else if (this.cover.VideoId != null)
                                    return $"cover [{this.cover.Id}] has no ref (type [{this.cover.CoverType}]) ext (video [{this.cover.VideoId}], actor [{this.cover.ActorId}]).";
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        return $"cover [{this.cover.Id}] has no ref (type [{this.cover.CoverType}]).";

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private enum ErrorType
        {
            RoleMissingSource,

            CoverMissingRef,

            MissingCover
        }
    }
}