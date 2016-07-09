using System.Threading.Tasks;

namespace JryVideo.Core.Managers.Upgrades
{
    public interface IPatch
    {

    }

    public interface IPatch<T> : IPatch
    {
        /// <summary>
        /// return true if upgrade success
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> UpgradeAsync(T item);
    }
}