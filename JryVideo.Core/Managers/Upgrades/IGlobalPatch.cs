using System.Threading.Tasks;

namespace JryVideo.Core.Managers.Upgrades
{
    public interface IGlobalPatch<T> : IPatch<T>
    {
        /// <summary>
        /// return true if upgrade success
        /// </summary>
        /// <param name="dataCenter"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> UpgradeAsync(DataCenter dataCenter, T item);
    }
}