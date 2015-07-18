using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class SeriesManager : JryObjectManager<JrySeries, ISeriesSet>
    {
        public event EventHandler<JrySeries> SeriesCreated;
        public event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoCreated;
        public event EventHandler<IEnumerable<ChangeEventArgs<JryVideoInfo>>> VideoInfoUpdated;
        public event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoRemoved;

        public DataCenter DataCenter { get; private set; }

        public SeriesManager(DataCenter dataCenter, ISeriesSet source)
            : base(source)
        {
            this.DataCenter = dataCenter;
        }

        public override async Task<bool> InsertAsync(JrySeries series)
        {
            if (await base.InsertAsync(series))
            {
                // Video
                this.SeriesCreated.BeginFire(this, series);

                if (series.Videos.Count > 0)
                    this.VideoInfoCreated.BeginFire(this, series.Videos.ToArray());

                return true;
            }

            return false;
        }

        public override async Task<bool> InsertAsync(IEnumerable<JrySeries> objs)
        {
            if (await base.InsertAsync(objs))
            {
                var videos = objs.SelectMany(z => z.Videos).ToArray();

                if (videos.Length > 0)
                    this.VideoInfoCreated.BeginFire(this, videos.ToArray());

                return true;
            }

            return false;
        }

        public async override Task<bool> UpdateAsync(JrySeries series)
        {
            var old = await this.FindAsync(series.Id);

            if (await base.UpdateAsync(series))
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
                    this.VideoInfoCreated.BeginFire(this, items);
                }

                if (bothIds.Length > 0)
                {
                    var items = bothIds.Select(id => new ChangeEventArgs<JryVideoInfo>(oldVideos[id], newVideos[id])).ToArray();
                    this.VideoInfoUpdated.BeginFire(this, items);
                }

                return true;
            }

            return false;
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
            return new VideoInfoManager(new VideoInfoJryEntitySetSet(this, obj));
        }

        private class VideoInfoJryEntitySetSet : IJasilyEntitySetProvider<JryVideoInfo, string>
        {
            public SeriesManager SeriesManager { get; set; }

            public JrySeries Series { get; set; }

            public VideoInfoJryEntitySetSet(SeriesManager SeriesManager, JrySeries series)
            {
                this.SeriesManager = SeriesManager;
                this.Series = series;
            }

            /// <summary>
            /// return a entities dictionary where match id.
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public async Task<IDictionary<string, JryVideoInfo>> FindAsync(IEnumerable<string> ids)
            {
                var array = ids.ToArray();
                return this.Series.Videos.Where(z => array.Contains(z.Id)).ToDictionary(z => z.Id);
            }

            public async Task<IEnumerable<JryVideoInfo>> ListAsync(int skip, int take)
            {
                return this.Series.Videos.ToArray();
            }

            /// <summary>
            /// return null if not found.
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public async Task<JryVideoInfo> FindAsync(string id)
            {
                return this.Series.Videos.FirstOrDefault(z => z.Id == id);
            }

            public async Task<bool> InsertAsync(JryVideoInfo entity)
            {
                this.Series.Videos.Add(entity);
                return await this.SeriesManager.UpdateAsync(this.Series);
            }

            public async Task<bool> UpdateAsync(JryVideoInfo entity)
            {
                var index = this.Series.Videos.FindIndex(z => z.Id == entity.Id);
                if (index == -1) return false;
                this.Series.Videos[index] = entity;
                return await this.SeriesManager.UpdateAsync(this.Series);
            }

            public async Task<bool> RemoveAsync(string id)
            {
                return this.Series.Videos.RemoveAll(z => z.Id == id) > 0 &&
                       await this.SeriesManager.UpdateAsync(this.Series);
            }

            public async Task<bool> InsertAsync(IEnumerable<JryVideoInfo> items)
            {
                this.Series.Videos.AddRange(items);
                return await this.SeriesManager.UpdateAsync(this.Series);
            }
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
            }

            return new SearchElement(SearchElement.ElementType.Text, text);
        }
    }
}