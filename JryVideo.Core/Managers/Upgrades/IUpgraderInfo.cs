using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades
{
    public interface IUpgraderInfo
    {
        Task<bool> UpgradeAsync(JrySeries series);

        Task<bool> UpgradeAsync(JryCover cover);
    }
}