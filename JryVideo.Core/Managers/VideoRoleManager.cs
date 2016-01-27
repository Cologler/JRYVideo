using JryVideo.Model;
using System.Data;

namespace JryVideo.Core.Managers
{
    public sealed class VideoRoleManager :
        AutoInsertVideoInfoAttachedManager<VideoRoleCollection, IJasilyEntitySetProvider<VideoRoleCollection, string>>
    {
        public VideoRoleManager(IJasilyEntitySetProvider<VideoRoleCollection, string> source)
            : base(source)
        {
        }
    }
}