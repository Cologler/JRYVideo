using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades
{
    public class Patch0002
    {
        public Task<bool> UpgradeAsync(JrySeries series)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> UpgradeAsync(JryCover cover)
        {
            return Task.FromResult(false);
        }
    }
}