using System.Collections.Generic;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class VideoManager : JryObjectManager<Model.JryVideo, IDataSourceProvider<Model.JryVideo>>
    {
        public VideoManager(IDataSourceProvider<Model.JryVideo> source)
            : base(source)
        {
        }

        public async void SeriesManager_VideoInfoCreated(object sender, IEnumerable<JryVideoInfo> e)
        {
            foreach (var info in e)
            {
                var v = Model.JryVideo.Build(info);
                v.BuildMetaData(true);

                await this.InsertAsync(v);
            }
        }

        public async void SeriesManager_VideoInfoRemoved(object sender, IEnumerable<JryVideoInfo> e)
        {
            foreach (var info in e)
            {
                await this.RemoveAsync(info.Id);
            }
        }
    }
}