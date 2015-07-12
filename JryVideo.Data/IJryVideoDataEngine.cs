using System.Data;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Data
{
    public interface IJryVideoDataEngine
    {
        string Name { get; }

        IJryVideoDataEngineInitializeParameters InitializeParametersInfo { get; }

        Task<bool> Initialize(JryVideoDataSourceProviderManagerMode mode);

        ISeriesSet GetSeriesDataSourceProvider();

        IJasilyEntitySetProvider<Model.JryVideo, string> GetVideoDataSourceProvider();

        IFlagSet GetCounterDataSourceProvider();

        ICoverSet GetCoverDataSourceProvider();

        IJasilyEntitySetProvider<JryArtist, string> GetArtistDataSourceProvider();

        IJasilyEntitySetProvider<JrySettingItem, string> GetSettingSet();
    }
}