using System.Collections.Generic;

namespace JryVideo.Data.DataSources
{
    public interface IDataSourceReaderProvider<out T>
    {
        IEnumerable<T> Get(int skip, int take);
    }
}