using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Jasily;
using Jasily.Collections.Generic;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Managers
{
    public class DatabaseHealthTester
    {
        private readonly List<string> messages = new List<string>();
        private readonly DataCenter dataCenter;
        private readonly Dictionary<string, CoverRef> covers = new Dictionary<string, CoverRef>();
        private readonly Dictionary<string, List<VideoInfoRef>> videoInfoRefs = new Dictionary<string, List<VideoInfoRef>>();
        private readonly Dictionary<string, SeriesRef> seriesRefs = new Dictionary<string, SeriesRef>();
        private readonly Dictionary<int, Dictionary<string, int>> flagRefs = new Dictionary<int, Dictionary<string, int>>();
        private readonly Dictionary<int, Dictionary<string, int>> originFlagRefs = new Dictionary<int, Dictionary<string, int>>();
        private readonly List<Error> Errors = new List<Error>();
        private readonly Dictionary<string, string> videoIdFromSeries = new Dictionary<string, string>();

        public DatabaseHealthTester(DataCenter dataCenter)
        {
            this.dataCenter = dataCenter;
        }

        [Conditional("DEBUG")]
        public async void RunOnDebugAsync() => await this.RunAsync(true);

        public async Task RunAsync(bool fix)
        {
            await Task.Run(async () =>
            {
                await this.BuildFlagAsync();
                await this.dataCenter.CoverManager.Source.CursorAsync(this.BuildCover);
                await this.BuildSeriesAsync();
                await this.BuildResourceAsync();
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

                foreach (var flagRef in this.flagRefs)
                {
                    var dict = this.originFlagRefs[flagRef.Key];
                    foreach (var i in flagRef.Value)
                    {
                        if (i.Value != 0)
                        {
                            var co = dict[i.Key];
                            this.Errors.Add(new FlagError(ErrorType.FlagCount, (JryFlagType)flagRef.Key, i.Key, co - i.Value, co));
                        }
                    }
                }

                foreach (var coverRef in this.covers.Values)
                {
                    if (coverRef.CoverParents.Count != 1)
                    {
                        if (coverRef.CoverParents.Count > 1)
                        {
                            this.messages.Add($"cover [{coverRef.Id}] ({coverRef.CoverType}) contain more then 1 ref:");
                            foreach (var refSource in coverRef.CoverParents)
                            {
                                this.messages.Add($"  {refSource.Type} [{refSource.Id}]");
                            }
                        }
                        else
                        {
                            this.Errors.Add(Error.FromCoverMissingParent(coverRef));
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

                this.Errors.Reset(this.Errors.OrderBy(z => z.GetOrderCode()).ToArray());

                // show message
                string id = null;
                foreach (var error in this.Errors)
                {
                    if (error.Id != id)
                    {
                        this.WriteLine("");
                        id = error.Id;
                        this.WriteLine($"---id [{id}]");
                    }
                    this.WriteLine(error.ToString());
                }

                // remove all missing source cover
                if (fix)
                {
                    foreach (var error in this.Errors) await error.FixAsync(this.dataCenter);
                }

                if (this.messages.Count + this.Errors.Count == 0)
                {
                    this.WriteLine("db safe!");
                }
                else
                {
                    this.WriteLine(this.messages.AsLines());

                    this.WriteLine($"total {this.Errors.Count} error.");
                    foreach (var group in this.Errors.GroupBy(z => z.Type).Select(z => z.ToList()).ToArray())
                    {
                        this.WriteLine($"{group.Count} of {group.Key}");
                    }

                    if (Debugger.IsAttached) Debugger.Break();
                }
            });
        }

        private void WriteLine(string message)
        {
            if (Debugger.IsAttached)
            {
                Debug.WriteLine(message);
            }
            else
            {
                Log.WriteLine(message);
            }
        }

        private async Task BuildFlagAsync()
        {
            await this.dataCenter.FlagManager.Source.CursorAsync(z =>
            {
                var type = (int)z.Type;
                var dest = this.flagRefs.GetOrCreateValue(type);
                if (string.IsNullOrEmpty(z.Value) || z.Id != JryFlag.BuildFlagId(z.Type, z.Value))
                {
                    if (Debugger.IsAttached) Debugger.Break();
                    Debug.Assert(false);
                }
                dest.Add(z.Value, z.Count);
            });
            foreach (var dictPair in this.flagRefs)
            {
                var d = new Dictionary<string, int>(dictPair.Value.Count);
                d.AddRange(dictPair.Value.ToList());
                this.originFlagRefs[dictPair.Key] = d;
            }
        }

        private async Task BuildSeriesAsync()
        {
            await this.dataCenter.SeriesManager.Source.CursorAsync(z =>
            {
                this.seriesRefs.Add(z.Id, new SeriesRef(z));
                if (z.Tags != null)
                {
                    this.ConnectToFlag(JryFlagType.SeriesTag, z.Tags);
                }
                foreach (var jryVideoInfo in z.Videos)
                {
                    this.videoIdFromSeries.Add(jryVideoInfo.Id, z.Id);
                    List<VideoInfoRef> videoInfoRefs;
                    if (!this.videoInfoRefs.TryGetValue(jryVideoInfo.Id, out videoInfoRefs))
                    {
                        videoInfoRefs = new List<VideoInfoRef>();
                        this.videoInfoRefs.Add(jryVideoInfo.Id, videoInfoRefs);
                    }
                    videoInfoRefs.Add(new VideoInfoRef(z.Id, jryVideoInfo));
                    this.ConnectToCover(z.Id, jryVideoInfo);

                    if (jryVideoInfo.Tags != null)
                    {
                        this.ConnectToFlag(JryFlagType.VideoTag, jryVideoInfo.Tags);
                    }

                    this.ConnectToFlag(JryFlagType.VideoYear, jryVideoInfo.Year.ToString());
                    this.ConnectToFlag(JryFlagType.VideoType, jryVideoInfo.Type);
                }

                z.Videos.Select(x => x.BackgroundImageAsCoverParent()).ForEach(x => this.ConnectToCover(z.Id, x));
            });
        }

        private async Task BuildResourceAsync()
        {
            await this.dataCenter.ResourceManager.Source.CursorAsync(z =>
            {
                if (z.VideoIds.Count == 0) throw new NotSupportedException();
                foreach (var id in z.VideoIds)
                {
                    if (!this.videoIdFromSeries.ContainsKey(id))
                    {
                        this.Errors.Add(new MissingVideoError(id, z.GetType()));
                    }
                }
                this.ConnectToFlag(JryFlagType.ResourceResolution, z.Resolution);
                if (!string.IsNullOrEmpty(z.Quality)) this.ConnectToFlag(JryFlagType.ResourceQuality, z.Quality);
                if (!string.IsNullOrEmpty(z.AudioSource)) this.ConnectToFlag(JryFlagType.ResourceAudioSource, z.AudioSource);
                this.ConnectToFlag(JryFlagType.ResourceExtension, z.Extension);
                this.ConnectToFlag(JryFlagType.ResourceFansub, z.Fansubs);
                this.ConnectToFlag(JryFlagType.ResourceSubTitleLanguage, z.SubTitleLanguages);
                this.ConnectToFlag(JryFlagType.ResourceTrackLanguage, z.TrackLanguages);
                this.ConnectToFlag(JryFlagType.ResourceTag, z.Tags);
            });
        }

        private void ConnectToFlag(JryFlagType type, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (Debugger.IsAttached) Debugger.Break();
                Debug.Assert(false);
            }

            var t = (int)type;
            var d = this.flagRefs.GetOrCreateValue(t);
            int n;
            if (d.TryGetValue(value, out n))
            {
                d[value] = n - 1;
            }
            else
            {
                this.Errors.Add(new FlagError(ErrorType.MissingFlag, type, value));
            }
        }

        private void ConnectToFlag(JryFlagType type, IEnumerable<string> value)
        {
            var t = (int)type;
            var d = this.flagRefs.GetOrCreateValue(t);
            int n;
            foreach (var v in value)
            {
                if (string.IsNullOrWhiteSpace(v))
                {
                    if (Debugger.IsAttached) Debugger.Break();
                    Debug.Assert(false);
                }

                if (d.TryGetValue(v, out n))
                {
                    d[v] = n - 1;
                }
                else
                {
                    this.Errors.Add(new FlagError(ErrorType.MissingFlag, type, v));
                }
            }
        }

        private void BuildCover(JryCover cover) => this.covers.Add(cover.Id, new CoverRef(cover));

        private void AddMessage(string msg) => this.messages.Add(msg);

        private void ConnectToCover(string rootObjectId, ICoverParent obj)
        {
            var coverId = obj.CoverId;
            if (coverId == null) return;
            CoverRef cover;
            if (this.covers.TryGetValue(coverId, out cover))
            {
                cover.CoverParents.Add(new CoverParentRef(obj));
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
                this.CoverType = obj.CoverType;
            }

            public List<CoverParentRef> CoverParents { get; } = new List<CoverParentRef>();

            public CoverType CoverType { get; }

            public string SeriesId { get; }

            public string VideoId { get; }

            public string ActorId { get; }
        }

        private class SeriesRef : ObjectRef
        {
            public SeriesRef(Series series)
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

        private class CoverParentRef
        {
            public CoverParentRef(ICoverParent obj)
            {
                this.Id = obj.Id;
                this.Type = obj.GetType().Name;
            }

            public string Type { get; }

            public string Id { get; }
        }

        private interface IError : IOrderable
        {
            Task FixAsync(DataCenter dataCenter);
        }

        private class Error : IError
        {
            public ErrorType Type { get; }

            public Error(ErrorType errorType)
            {
                this.Type = errorType;
            }

            public string Id { get; protected set; } = string.Empty;

            public static Error RoleMissingSource(string roleCollectionId) => new Error(ErrorType.RoleMissingSource) { Id = roleCollectionId };

            public static Error FromCoverMissingParent(CoverRef cover) => new CoverError(ErrorType.CoverMissingParent, cover);

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

            public virtual Task FixAsync(DataCenter dataCenter) => Task.FromResult(0);

            public int GetOrderCode() => this.Id?.GetHashCode() ?? 0;
        }

        private class CoverError : Error
        {
            private readonly CoverRef cover;

            public CoverError(ErrorType errorType, CoverRef cover)
                : base(errorType)
            {
                this.cover = cover;

                if (errorType != ErrorType.CoverMissingParent)
                {
                    throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null);
                }

                switch (cover.CoverType)
                {
                    case CoverType.Role:
                    case CoverType.Artist:
                        this.Id = cover.ActorId;
                        this.Id = cover.ActorId;
                        break;

                    case CoverType.Video:
                    case CoverType.Background:
                        this.Id = cover.VideoId;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                this.Id = this.Id ?? cover.Id;
            }

            public override async Task FixAsync(DataCenter dataCenter)
            {
                var oldId = this.cover.Id;

                switch (this.Type)
                {
                    case ErrorType.CoverMissingParent:
                        switch (this.cover.CoverType)
                        {
                            case CoverType.Artist:
                                break;

                            case CoverType.Video:
                                break;

                            case CoverType.Background:
                                break;

                            case CoverType.Role:
                                if (this.cover.ActorId != null)
                                {
                                    if (this.cover.VideoId != null)
                                    {
                                        if (this.cover.SeriesId != null)
                                        {

                                        }
                                        else
                                        {
                                            var roleCol = await dataCenter.VideoRoleManager.FindAsync(this.cover.VideoId);
                                            var matchs = roleCol.Roles().Where(z => z.ActorId == this.cover.ActorId).ToArray();
                                            if (matchs.Length != 1) return;
                                            var cover = await dataCenter.CoverManager.FindAsync((matchs[0] as ICoverParent).CoverId);
                                            if (cover != null) return;
                                            cover = await dataCenter.CoverManager.FindAsync(this.cover.Id);
                                            cover.Id = (matchs[0] as ICoverParent).CoverId;
                                            await dataCenter.CoverManager.InsertOrUpdateAsync(cover);
                                            await dataCenter.CoverManager.RemoveAsync(oldId);
                                        }
                                    }
                                    else
                                    {

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

            public override string ToString()
            {
                switch (this.Type)
                {
                    case ErrorType.CoverMissingParent:
                        switch (this.cover.CoverType)
                        {
                            case CoverType.Artist:
                                break;

                            case CoverType.Video:
                                break;

                            case CoverType.Background:
                                return $"cover [{this.cover.Id}] has no ref (type [{this.cover.CoverType}]) ext (video [{this.cover.VideoId}]).";

                            case CoverType.Role:
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

        private class FlagError : Error
        {
            private readonly JryFlagType type;
            private readonly string value;
            private readonly int count;
            private readonly int actualCount;

            public FlagError(ErrorType errorType, JryFlagType type, string value)
                : base(errorType)
            {
                this.type = type;
                this.value = value;
                this.Id = type.ToString();
            }

            public FlagError(ErrorType errorType, JryFlagType type, string value, int count, int actualCount)
                : base(errorType)
            {
                this.type = type;
                this.value = value;
                this.count = count;
                this.actualCount = actualCount;
                this.Id = type.ToString();
            }

            public override async Task FixAsync(DataCenter dataCenter)
            {
                switch (this.Type)
                {
                    case ErrorType.FlagCount:
                        var flag = await dataCenter.FlagManager.FindAsync(JryFlag.BuildFlagId(this.type, this.value));
                        if (flag != null)
                        {
                            flag.Count = this.count;
                            await dataCenter.FlagManager.UpdateAsync(flag);
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public override string ToString()
            {
                var flagStr = $"{this.type}.{this.value}";
                switch (this.Type)
                {
                    case ErrorType.MissingFlag:
                        return "missing flag " + flagStr;

                    case ErrorType.FlagCount:
                        return $"flag {flagStr} count error, [{this.actualCount}] should be [{this.count}].";

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private sealed class MissingVideoError : Error
        {
            private readonly string id;
            private readonly Type type;

            public MissingVideoError(string id, Type type)
                : base(ErrorType.Default)
            {
                this.id = id;
                this.type = type;
            }

            #region Overrides of Error

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>
            /// A string that represents the current object.
            /// </returns>
            public override string ToString() => $"{this.type.Name}[{this.id}] cannot map from series.";

            #endregion
        }

        private enum ErrorType
        {
            Default,

            RoleMissingSource,

            CoverMissingParent,

            ParentMissingCover,

            MissingFlag,

            FlagCount
        }
    }
}