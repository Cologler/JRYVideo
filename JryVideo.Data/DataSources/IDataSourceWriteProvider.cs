using System.Collections.Generic;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface IDataSourceWriteProvider<in T>
    {
        Task<bool> InsertAsync(T value);

        Task<bool> InsertAsync(IEnumerable<T> items);

        Task<bool> UpdateAsync(T value);

        Task<bool> RemoveAsync(string id);
    }
}