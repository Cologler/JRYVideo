using JryVideo.Data.DataSources;
using JryVideo.Model;
using System.Data;
using System.Threading.Tasks;

namespace JryVideo.Data
{
    public interface IJryVideoDataEngine
    {
        string Name { get; }

        IJryVideoDataEngineInitializeParameters InitializeParametersInfo { get; }

        Task<bool> Initialize(JryVideoDataSourceProviderManagerMode mode);

        Task<bool> HasPasswordAsync();

        Task<bool> PasswordAsync(string password);

        ISeriesSet GetSeriesSet();

        IFlagableSet<Model.JryVideo> GetVideoSet();

        IFlagSet GetFlagSet();

        ICoverSet GetCoverSet();

        IArtistSet GetArtistSet();

        IVideoRoleCollectionSet GetVideoRoleInfoSet();

        IJasilyEntitySetProvider<JrySettingItem, string> GetSettingSet();
    }
}