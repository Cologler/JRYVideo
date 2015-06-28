using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class VideoInfoManager : JryObjectManager<JryVideoInfo, IDataSourceProvider<JryVideoInfo>>
    {
        public VideoInfoManager(IDataSourceProvider<JryVideoInfo> source)
            : base(source)
        {
        }
    }
}