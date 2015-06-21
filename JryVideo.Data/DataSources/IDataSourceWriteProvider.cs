using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface IDataSourceWriteProvider<in T>
    {
        Task<bool> InsertAsync(T value);

        Task<bool> UpdateAsync(T value);

        Task<bool> RemoveAsync(T value);
    }
}