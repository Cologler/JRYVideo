using System.Security.Cryptography.X509Certificates;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Data
{
    public interface IJryVideoDataSourceSetProvider
    {
        string Name { get; }

        IDataSourceProvider<JrySeries> GetSeriesDataSourceProvider();

        IDataSourceProvider<JryCover> GetCoverDataSourceProvider();
    }
}