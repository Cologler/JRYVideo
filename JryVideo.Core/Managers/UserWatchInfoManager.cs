using Jasily.Data;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public sealed class UserWatchInfoManager :
        AutoInsertVideoInfoAttachedManager<UserWatchInfo, IJasilyEntitySetProvider<UserWatchInfo, string>>
    {
        public UserWatchInfoManager(IJasilyEntitySetProvider<UserWatchInfo, string> source)
            : base(source)
        {
        }
    }
}