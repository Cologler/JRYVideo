using System.Collections.Generic;
using System.Linq;
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
            await this.InsertAsync(e.Select(i => 
            {
                var v = Model.JryVideo.Build(i);
                v.BuildMetaData(true);
                return v;
            }));
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