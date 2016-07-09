using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades
{
    public class Patch0000 : IPatch<JrySeries>, IPatch<JryCover>
    {
        public Task<bool> UpgradeAsync(JrySeries series) => Task.FromResult(true);

        public Task<bool> UpgradeAsync(JryCover cover) => Task.FromResult(true);
    }
}