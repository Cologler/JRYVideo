using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class SeriesManager : JryObjectManager<JrySeries, ISeriesDataSourceProvider>
    {
        public event EventHandler<JrySeries> SeriesCreated;
        public event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoRemoved;
        public event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoCreated;

        public DataCenter DataCenter { get; private set; }

        public SeriesManager(DataCenter dataCenter, ISeriesDataSourceProvider source)
            : base(source)
        {
            this.DataCenter = dataCenter;
        }

        public async override Task<bool> InsertAsync(JrySeries series)
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

        public async override Task<bool> UpdateAsync(JrySeries series)
        {
            var old = await this.FindAsync(series.Id);

            if (await base.UpdateAsync(series))
            {
                // Video
                var oldVideos = old.Videos.ToDictionary(z => z.Id);
                var newVideos = series.Videos.ToDictionary(z => z.Id);

                var oldVideoIds = oldVideos.Keys.ToArray();
                var newVideoIds = newVideos.Keys.ToArray();

                var onlyOld = oldVideoIds.Except(newVideoIds).Select(id => oldVideos[id]).ToArray();
                var onlyNew = newVideoIds.Except(oldVideoIds).Select(id => newVideos[id]).ToArray();

                if (onlyOld.Length > 0)
                    this.VideoInfoRemoved.BeginFire(this, onlyOld);

                if (onlyNew.Length > 0)
                    this.VideoInfoCreated.BeginFire(this, onlyNew);

                return true;
            }

            return false;
        }

        public async override Task<IEnumerable<JrySeries>> LoadAsync()
        {
            return await this.Source.QueryAsync(0, Int32.MaxValue);
        }

        public async Task<IEnumerable<JrySeries>> QueryAsync(string searchText)
        {
            return await this.Source.QueryByNameAsync(searchText, 0, Int32.MaxValue);
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
            return new VideoInfoManager(new VideoInfoDataSourceProvider(this, obj));
        }

        private class VideoInfoDataSourceProvider : IDataSourceProvider<JryVideoInfo>
        {
            public SeriesManager SeriesManager { get; set; }

            public JrySeries Series { get; set; }

            public VideoInfoDataSourceProvider(SeriesManager SeriesManager, JrySeries series)
            {
                this.SeriesManager = SeriesManager;
                this.Series = series;
            }

            public async Task<IEnumerable<JryVideoInfo>> QueryAsync(int skip, int take)
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

            public async Task<bool> InsertAsync(JryVideoInfo value)
            {
                this.Series.Videos.Add(value);
                return await this.SeriesManager.UpdateAsync(this.Series);
            }

            public async Task<bool> UpdateAsync(JryVideoInfo value)
            {
                var index = this.Series.Videos.FindIndex(z => z.Id == value.Id);
                if (index == -1) return false;
                this.Series.Videos[index] = value;
                return await this.SeriesManager.UpdateAsync(this.Series);
            }

            public async Task<bool> RemoveAsync(string id)
            {
                if (this.Series.Videos.RemoveAll(z => z.Id == id) > 0)
                {
                    return await this.SeriesManager.UpdateAsync(this.Series);
                }
                return false;
            }


            public async Task<bool> InsertAsync(IEnumerable<JryVideoInfo> items)
            {
                this.Series.Videos.AddRange(items);
                return await this.SeriesManager.UpdateAsync(this.Series);
            }
        }
    }
}