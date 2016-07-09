using System.Threading.Tasks;

namespace JryVideo.Common
{
    public interface IAutoGenerateCoverProvider<in T> where T : Model.Interfaces.ICoverParent
    {
        /// <summary>
        /// return true if success.
        /// </summary>
        /// <returns></returns>
        Task<bool> GenerateAsync(T source);
    }
}