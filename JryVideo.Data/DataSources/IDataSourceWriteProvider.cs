using System.Collections.Generic;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface IDataSourceWriteProvider<T>
    {
        Task<bool> InsertAsync(T entity);

        Task<bool> InsertAsync(IEnumerable<T> items);

        Task<bool> UpdateAsync(T entity);

        Task<bool> RemoveAsync(string id);
    }
}