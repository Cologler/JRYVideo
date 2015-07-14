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

        ISeriesSet GetSeriesSet();

        IJasilyEntitySetProvider<Model.JryVideo, string> GetVideoSet();

        IFlagSet GetFlagSet();

        ICoverSet GetCoverSet();

        IJasilyEntitySetProvider<JryArtist, string> GetArtistSet();

        IJasilyEntitySetProvider<JrySettingItem, string> GetSettingSet();
    }
}