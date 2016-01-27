using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class VideoRoleViewModel : HasCoverViewModel<JryVideoRole>
    {
        public VideoRoleViewModel(JryVideoRole source)
            : base(source)
        {
        }

        protected override Task<bool> TryAutoAddCoverAsync()
        {
            return Task.FromResult(false);
        }
    }
}