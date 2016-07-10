using System.Threading.Tasks;
using JryVideo.Core.Managers;

namespace JryVideo.Common
{
    public interface IAutoGenerateCoverProvider<in T> where T : Model.Interfaces.ICoverParent
    {
        /// <summary>
        /// return true if success.
        /// </summary>
        /// <param name="dataCenter"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<bool> GenerateAsync(DataCenter dataCenter, T source);
    }
}