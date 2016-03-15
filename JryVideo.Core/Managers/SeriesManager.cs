using JryVideo.Data;
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
        public event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoCreated;
        public event EventHandler<IEnumerable<ChangingEventArgs<JryVideoInfo>>> VideoInfoUpdated;
        public event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoRemoved;

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
        {
            var items = await new Query(this, searchText).StartQuery(skip, take);

            return items;
        }

        public async Task<IEnumerable<JrySeries>> ListTrackingAsync() => await this.Source.ListTrackingAsync();

        public static void BuildSeriesMetaData(JrySeries series)
        {
            BuildObjectMetaData(series);
            foreach (var jryVideoInfo in series.Videos)
            {
                BuildObjectMetaData(jryVideoInfo);
            }
        }

        private static void BuildObjectMetaData(JryObject obj)
        {
            if (obj != null && !obj.IsMetaDataBuilded())
            {
                obj.BuildMetaData();
            }
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

            public Query(SeriesManager seriesManager, string originText)
            {
                this.seriesManager = seriesManager;
                this.QueryParameters = ParseQueryParameters(originText).ToArray();
            }

            public JrySeries.QueryParameter[] QueryParameters { get; }

            private static IEnumerable<JrySeries.QueryParameter> ParseQueryParameters(string originText)
            {
                if (string.IsNullOrWhiteSpace(originText))
                {
                    yield return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.Any, originText);
                    yield break;
                }

                var p = ParseQueryParameterWithKeyword(originText);
                if (p != null)
                {
                    yield return p.Value;
                    yield break;
                }

                yield return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.OriginText, originText);

                if (originText.All(char.IsDigit))
                {
                    if (originText.Length == 4)
                    {
                        var number = int.Parse(originText);
                        if (number > 1900 && number < 2100)
                        {
                            yield return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.VideoYear, originText);
                        }
                    }

                    if (originText.Length > 5)
                    {
                        yield return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.DoubanId, originText);
                    }

                    yield break;
                }

                if (originText.StartsWith("tt") && originText.Substring(2).All(char.IsDigit))
                {
                    yield return new JrySeries.QueryParameter(originText, JrySeries.QueryMode.ImdbId, originText);
                }
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
                    }
                }

                return null;
            }

            public async Task<IEnumerable<JrySeries>> StartQuery(int skip, int take)
            {
                var list = new List<JrySeries>(take);
                Dictionary<string, JrySeries> sets = null;
                foreach (var queryParameter in this.QueryParameters)
                {
                    if (queryParameter.Mode == JrySeries.QueryMode.Any)
                    {
                        Debug.Assert(this.QueryParameters.Length == 1);
                        list.AddRange(await this.seriesManager.LoadAsync(skip, take));
                    }
                    else
                    {
                        if (sets == null) sets = new Dictionary<string, JrySeries>();

                        foreach (var item in await this.seriesManager.Source.QueryAsync(queryParameter, skip, take))
                        {
                            if (!sets.ContainsKey(item.Id))
                            {
                                list.Add(item);
                                sets.Add(item.Id, item);
                            }
                        }
                    }
                }

                return list;
            }

            public bool IsMatch(JrySeries series, JryVideoInfo video)
            {
                if (series == null) throw new ArgumentNullException(nameof(series));
                if (video == null) throw new ArgumentNullException(nameof(video));


                foreach (var queryParameter in this.QueryParameters)
                {
                    switch (queryParameter.Mode)
                    {
                        case JrySeries.QueryMode.Any:
                            Debug.Assert(this.QueryParameters.Length == 1);
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

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return false;
            }
        }
    }
}