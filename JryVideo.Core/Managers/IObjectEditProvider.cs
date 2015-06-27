using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public interface IObjectEditProvider<in T>
    {
        Task<bool> InsertAsync(T obj);

        Task<bool> UpdateAsync(T obj);
    }
}