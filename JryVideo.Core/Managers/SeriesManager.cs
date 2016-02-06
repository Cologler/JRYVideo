using JryVideo.Data;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Data;
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
            return await this.Source.QueryAsync(
                ParseSearchText(searchText.ThrowIfNullOrEmpty("searchText")), skip, take);
        }

        public async Task<IEnumerable<JrySeries>> ListTrackingAsync()
        {
            return await this.Source.ListTrackingAsync();
        }

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

        private static SearchElement ParseSearchText(string text)
        {
            var index = text.IndexOf(':');

            if (index < 1 || index == text.Length - 1)
                return new SearchElement(SearchElement.ElementType.Text, text);

            switch (text.Substring(0, index).ToLower())
            {
                case "series-id":
                    return new SearchElement(SearchElement.ElementType.SeriesId, text.Substring(index + 1));
                case "video-id":
                    return new SearchElement(SearchElement.ElementType.VideoId, text.Substring(index + 1));
                case "entity-id":
                    return new SearchElement(SearchElement.ElementType.EntityId, text.Substring(index + 1));
                case "douban-id":
                    return new SearchElement(SearchElement.ElementType.DoubanId, text.Substring(index + 1));
                case "tag":
                    return new SearchElement(SearchElement.ElementType.Tag, text.Substring(index + 1));
            }

            return new SearchElement(SearchElement.ElementType.Text, text);
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
    }
}