using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Core.Managers.Upgrades
{
    public interface IUpgraderInfo
    {
        int Version { get; }

        Task UpgradeAsync(JrySeries series);

        Task UpgradeAsync(JryCover cover);
    }
}