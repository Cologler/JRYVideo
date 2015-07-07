using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IDataSourceProvider<T> : IDataSourceReaderProvider<T>, IDataSourceWriteProvider<T>
        where T : JryObject
    {
    }
}