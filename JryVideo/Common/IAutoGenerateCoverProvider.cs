using System.Threading.Tasks;
using JryVideo.Core.Managers;
using JryVideo.Model.Interfaces;

namespace JryVideo.Common
{
    public interface IAutoGenerateCoverProvider
    {
        /// <summary>
        /// return true if success.
        /// </summary>
        /// <param name="dataCenter"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        Task<bool> GenerateAsync(DataCenter dataCenter, ICoverParent source);
    }
}