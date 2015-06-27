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
    public class SeriesManager : JryObjectManager<JrySeries, IDataSourceProvider<JrySeries>>
    {
        public event EventHandler<JrySeries> SeriesCreated;
        public event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoRemoved;
        public event EventHandler<IEnumerable<JryVideoInfo>> VideoInfoCreated;

        public DataCenter DataCenter { get; private set; }

        public SeriesManager(DataCenter dataCenter, IDataSourceProvider<JrySeries> source)
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

        public static void BuildSeriesMetaData(JrySeries series)
        {
            SeriesAction(series, BuildObjectMetaData);
        }

        private static void BuildObjectMetaData(JryObject obj)
        {
            if (obj != null && !obj.IsMetaDataBuilded())
            {
                obj.BuildMetaData();
            }
        }

        private static void SeriesAction(JrySeries series, Action<JryObject> action)
        {
            if (series == null) return;

            var func = new Func<JryObject, bool>(z =>
            {
                action(z);
                return true;
            });

            var j = SeriesFunc(series, func).ToArray();
        }
        private static IEnumerable<T> SeriesFunc<T>(JrySeries series, Func<JryObject, T> func)
        {
            if (series == null) return Enumerable.Empty<T>();

            if (series.Videos == null)
                return new [] { func(series) };

            return new[] { func(series) }
                .Concat(series.Videos.Select(z => func(z)))
                .ToArray();
        }

        public async Task<bool> MergeAsync(JrySeries dest, JrySeries source)
        {
            return await Task.Run(async () =>
            {
                dest.Names.AddRange(source.Names);
                dest.Names = dest.Names.Distinct().ToList();

                dest.Videos.AddRange(source.Videos);

                return await this.UpdateAsync(dest);
            });
        }
    }
}