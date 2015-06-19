using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IDataSourceReaderProvider<T>
        where T : JryObject
    {
        Task<IEnumerable<T>> QueryAsync(int skip, int take);

        Task<T> QueryAsync(Guid id);
    }
}