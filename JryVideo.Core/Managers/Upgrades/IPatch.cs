using System.Threading.Tasks;

namespace JryVideo.Core.Managers.Upgrades
{
    public interface IPatch
    {

    }

    public interface IPatch<T> : IPatch
    {
        Task<bool> UpgradeAsync(T item);
    }
}