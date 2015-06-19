using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Data
{
    public interface IJryVideoDataSourceProviderManager
    {
        string Name { get; }

        IDataSourceProviderManagerInitializeParameters InitializeParametersInfo { get; }

        Task<bool> Initialize();

        IDataSourceProvider<JrySeries> GetSeriesDataSourceProvider();

        ICoverDataSourceProvider GetCoverDataSourceProvider();

        IDataSourceProvider<JryArtist> GetArtistDataSourceProvider();
    }
}