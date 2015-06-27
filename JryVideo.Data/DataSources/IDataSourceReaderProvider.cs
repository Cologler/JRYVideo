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

        /// <summary>
        /// return null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> FindAsync(string id);
    }
}