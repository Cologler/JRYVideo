using System.Security.Cryptography.X509Certificates;

namespace JryVideo.Data.DataSources
{
    public interface IDataSourceProvider<T> : IDataSourceReaderProvider<T>, IDataSourceWriteProvider<T>
    {
    }
}