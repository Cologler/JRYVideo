using System.Data;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class VideoInfoManager : JryObjectManager<JryVideoInfo, IJasilyEntitySetProvider<JryVideoInfo, string>>
    {
        public VideoInfoManager(IJasilyEntitySetProvider<JryVideoInfo, string> source)
            : base(source)
        {
        }
    }
}