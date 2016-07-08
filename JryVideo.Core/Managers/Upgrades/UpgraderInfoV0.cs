using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades
{
    public class UpgraderInfoV0 : IUpgraderInfo
    {
        public int Version => 0;

        public Task UpgradeAsync(JrySeries series) => Task.FromResult(0);

        public Task UpgradeAsync(JryCover cover) => Task.FromResult(0);
    }
}