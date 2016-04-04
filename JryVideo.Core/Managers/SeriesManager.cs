using JryVideo.Data.DataSources;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.EventArgses;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public class SeriesManager : JryObjectManager<JrySeries, ISeriesSet>
    {
        internal event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoCreated;
        internal event EventHandler<IEnumerable<ChangingEventArgs<JryVideoInfo>>> VideoInfoUpdated;
        internal event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoRemoved;

        public DataCenter DataCenter { get; private set; }

        public SeriesManager(DataCenter dataCenter, ISeriesSet source)
            : base(source)
        {
            this.DataCenter = dataCenter;
        }

        public override Task<bool> InsertAsync(JrySeries series) => this.InsertAsync(series, true);

        private async Task<bool> InsertAsync(JrySeries series, bool isRaiseEvent)
        {
            if (await base.InsertAsync(series))
            {
                if (isRaiseEvent)
                {
                    if (series.Videos.Count > 0)
                        this.VideoInfoCreated?.BeginFire(this, series.Videos.ToArray());
                }

                return true;
            }

            return false;
        }

        protected override Task<bool> InsertAsync(IEnumerable<JrySeries> objs) => this.InsertAsync(objs, true);

        private async Task<bool> InsertAsync(IEnumerable<JrySeries> objs, bool isRaiseEvent)
        {
            if (await base.InsertAsync(objs))
            {
                if (isRaiseEvent)
                {
                    var videos = objs.SelectMany(z => z.Videos).ToArray();

                    if (videos.Length > 0)
                        this.VideoInfoCreated.BeginFire(this, videos.ToArray());
                }

                return true;
            }

            return false;
        }

        public override Task<bool> UpdateAsync(JrySeries series) => this.UpdateAsync(series, true);

        private async Task<bool> UpdateAsync(JrySeries series, bool isRaiseEvent)
        {
            var old = await this.FindAsync(series.Id);

            if (await base.UpdateAsync(series))
            {
                if (isRaiseEvent)
                {
                    // Video
                    // for video info, just need add and remove, don't need update event.
                    var oldVideos = old.Videos.ToDictionary(z => z.Id);
                    var newVideos = series.Videos.ToDictionary(z => z.Id);

                    var oldVideoIds = oldVideos.Keys.ToArray();
                    var newVideoIds = newVideos.Keys.ToArray();

                    var onlyOldIds = oldVideoIds.Except(newVideoIds).ToArray(); // 只在 old
                    var onlyNewIds = newVideoIds.Except(oldVideoIds).ToArray(); // 只在 new
                    var bothIds = oldVideoIds.Intersect(newVideoIds).ToArray(); // 交集

                    if (onlyOldIds.Length > 0)
                    {
                        var items = onlyOldIds.Select(id => oldVideos[id]).ToArray();
                        this.VideoInfoRemoved.BeginFire(this, items);
                    }

                    if (onlyNewIds.Length > 0)
                    {
                        var items = onlyNewIds.Select(id => newVideos[id]).ToArray();
                        this.VideoInfoCreated?.BeginFire(this, items);
                    }

                    if (bothIds.Length > 0)
                    {
                        var items = bothIds.Select(id => new ChangingEventArgs<JryVideoInfo>(oldVideos[id], newVideos[id])).ToArray();
                        this.VideoInfoUpdated?.BeginFire(this, items);
                    }
                }

                return true;
            }

            return false;
        }

        public override Task<bool> RemoveAsync(string id) => this.RemoveAsync(id, true);

        private async Task<bool> RemoveAsync(string id, bool isRaiseEvent)
        {
            var item = await this.FindAsync(id);
            if (item != null)
            {
                item.Videos = new List<JryVideoInfo>();
                await this.UpdateAsync(item, isRaiseEvent);
            }
            return await base.RemoveAsync(id);
        }

        public async Task<IEnumerable<JrySeries>> QueryAsync(string searchText, int skip, int take)
            => await this.GetQuery(searchText).StartQuery(skip, take);

        public Query GetQuery(string searchText) => new Query(this, searchText);

        public async Task<IEnumerable<JrySeries>> ListTrackingAsync() => await this.Source.ListTrackingAsync();

        public static void BuildSeriesMetaData(JrySeries series)
        {
            BuildObjectMetaData(series);
            series.Videos.ForEach(BuildObjectMetaData);
        }

        private static void BuildObjectMetaData(JryObject obj)
        {
            if (obj?.IsMetaDataBuilded() == false) obj.BuildMetaData();
        }

        public VideoInfoManager GetVideoInfoManager(JrySeries obj)
        {
            var manager = new VideoInfoManager(new SubObjectSetProvider<JrySeries, JryVideoInfo>(this, obj));
            manager.CoverParentRemoving += (sender, parent) => this.OnCoverParentRemoving(parent, sender);
            return manager;
        }

        internal override Task<CombineResult> CanCombineAsync(JrySeries to, JrySeries @from)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (from == null) throw new ArgumentNullException(nameof(from));

            return Task.FromResult(this.CanCombine(to, from));
        }

        private CombineResult CanCombine(JrySeries to, JrySeries @from)
        {
            if (to.TheTVDBId != null && from.TheTVDBId != null && to.TheTVDBId != from.TheTVDBId)
            {
                return CombineResult.False("have diff TheTVDB id.");
            }

            if (to.ImdbId != null && from.ImdbId != null && to.ImdbId != from.ImdbId)
            {
                return CombineResult.False("have diff douban id.");
            }

            return CombineResult.True;
        }

        internal override async Task<CombineResult> CombineAsync(JrySeries to, JrySeries @from)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (from == null) throw new ArgumentNullException(nameof(from));

            var result = await this.CanCombineAsync(to, from);

            if (result.CanCombine)
            {
                to.CombineFrom(from);
                await this.UpdateAsync(to, false);
                await this.UpdateAsync(from, false);
                await this.RemoveAsync(from.Id);
            }

            return result;
        }

        public sealed class Query
        {
            private readonly SeriesManager seriesManager;
            private readonly int[] starArray;

            public Query(SeriesManager seriesManager, string originText)
            {
                this.seriesManager = seriesManager;
                this.QueryParameter = ParseQueryParameter(originText);
                if (this.QueryParameter.Mode == JrySeries.QueryMode.Star)
                {
                    this.starArray = JrySeries.QueryParameter.GetStar(this.QueryParameter.Keyword);
                }
            }

            public JrySeries.QueryParameter QueryParameter { get; }

            private static JrySeries.QueryParameter ParseQueryParameter(string originText)
            {
                if (string.IsNullOrWhiteSpace(originText))
                {
                    return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.Any, originText);
                }

                var p = ParseQueryParameterWithKeyword(originText);
                if (p != null)
                {
                    return p.Value;
                }

                return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.OriginText, originText);
            }

            private static JrySeries.QueryParameter? ParseQueryParameterWithKeyword(string originText)
            {
                var index = originText.IndexOf(':');

                if (index >= 1 && index != originText.Length - 1)
                {
                    switch (originText.Substring(0, index).ToLower())
                    {
                        case "series-id":
                            return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.SeriesId,
                                originText.Substring(index + 1));

                        case "video-id":
                            return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.VideoId,
                                originText.Substring(index + 1));

                        case "entity-id":
                            return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.EntityId,
                                originText.Substring(index + 1));

                        case "douban-id":
                            return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.DoubanId,
                                originText.Substring(index + 1));

                        case "tag":
                            return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.Tag,
                                originText.Substring(index + 1));

                        case "type":
                            return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.VideoType,
                                originText.Substring(index + 1));

                        case "year":
                            var year = originText.Substring(index + 1);
                            if (JrySeries.QueryParameter.CanBeYear(year))
                            {
                                return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.VideoYear,
                                    year);
                            }
                            break;

                        case "imdb-id":
                            return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.ImdbId,
                                originText.Substring(index + 1));

                        case "star":
                            var star = originText.Substring(index + 1);
                            if (JrySeries.QueryParameter.CanBeStar(star))
                            {
                                return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.Star,
                                    star);
                            }
                            break;
                    }
                }

                return null;
            }

            public async Task<IEnumerable<JrySeries>> StartQuery(int skip, int take)
            {
                var queryParameter = this.QueryParameter;
                return queryParameter.Mode == JrySeries.QueryMode.Any
                    ? (await this.seriesManager.LoadAsync(skip, take)).ToList(take)
                    : (await this.seriesManager.Source.QueryAsync(queryParameter, skip, take)).ToList(take);
            }

            public bool IsMatch(JrySeries series, JryVideoInfo video)
            {
                if (series == null) throw new ArgumentNullException(nameof(series));
                if (video == null) throw new ArgumentNullException(nameof(video));

                var queryParameter = this.QueryParameter;
                switch (queryParameter.Mode)
                {
                    case JrySeries.QueryMode.Any:
                        return true;

                    case JrySeries.QueryMode.OriginText:
                        if (series.Names.Any(z => z.Contains(queryParameter.Keyword, StringComparison.OrdinalIgnoreCase)) ||
                            video.Names.Any(z => z.Contains(queryParameter.Keyword, StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }
                        break;

                    case JrySeries.QueryMode.SeriesId:
                        if (series.Id == queryParameter.Keyword) return true;
                        break;

                    case JrySeries.QueryMode.VideoId:
                        if (video.Id == queryParameter.Keyword) return true;
                        break;

                    case JrySeries.QueryMode.EntityId:
                        return true;

                    case JrySeries.QueryMode.DoubanId:
                        if (video.DoubanId == queryParameter.Keyword) return true;
                        break;

                    case JrySeries.QueryMode.Tag:
                        if ((series.Tags != null && series.Tags.Any(z => z == queryParameter.Keyword)) ||
                            (video.Tags != null && video.Tags.Any(z => z == queryParameter.Keyword)))
                            return true;
                        break;

                    case JrySeries.QueryMode.VideoType:
                        if (video.Type == queryParameter.Keyword) return true;
                        break;

                    case JrySeries.QueryMode.VideoYear:
                        if (video.Year.ToString() == queryParameter.Keyword) return true;
                        break;

                    case JrySeries.QueryMode.ImdbId:
                        if (video.ImdbId == queryParameter.Keyword) return true;
                        break;

                    case JrySeries.QueryMode.Star:
                        return this.starArray.Contains(video.Star);

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return false;
            }
        }

        public void Initialize(DataCenter dataCenter)
        {
            dataCenter.FlagManager.FlagChanged += this.FlagManager_FlagChanged;
        }

        private async void FlagManager_FlagChanged(object sender, EventArgs<JryFlagType, string, string> e)
        {
            var type = e.Value1;
            var oldValue = e.Value2;
            var newValue = e.Value3;

            JrySeries.QueryParameter queryParameter;
            switch (type)
            {
                case JryFlagType.SeriesTag:
                case JryFlagType.VideoTag:
                    queryParameter = new JrySeries.QueryParameter(oldValue, JrySeries.QueryMode.Tag, oldValue);
                    break;

                case JryFlagType.VideoType:
                    queryParameter = new JrySeries.QueryParameter(oldValue, JrySeries.QueryMode.VideoType, oldValue);
                    break;

                default:
                    if ((int)type < 20) throw new NotSupportedException();
                    return;
            }

            var series = (await this.Source.QueryAsync(queryParameter, 0, int.MaxValue)).ToArray();

            switch (type)
            {
                case JryFlagType.SeriesTag:
                    foreach (var item in series.Where(z => z.Tags != null && z.Tags.Contains(oldValue)))
                    {
                        if (item.Tags?.Remove(oldValue) == true) item.Tags.Add(newValue);
                        await this.UpdateAsync(item);
                    }
                    break;

                case JryFlagType.VideoTag:
                    foreach (var item in series)
                    {
                        Debug.Assert(item.Videos != null);
                        var hasChanged = false;
                        foreach (var info in item.Videos.Where(z => z.Tags?.Contains(oldValue) == true))
                        {
                            if (info.Tags?.Remove(oldValue) == true) info.Tags.Add(newValue);
                            hasChanged = true;
                        }
                        if (hasChanged) await this.UpdateAsync(item);
                    }
                    break;

                case JryFlagType.VideoType:
                    foreach (var item in series)
                    {
                        Debug.Assert(item.Videos != null);
                        var hasChanged = false;
                        foreach (var info in item.Videos.Where(z => z.Type == oldValue))
                        {
                            info.Type = newValue;
                            hasChanged = true;
                        }
                        if (hasChanged) await this.UpdateAsync(item);
                    }
                    break;
            }
        }
    }
}