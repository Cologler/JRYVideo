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

        Task<bool> HasPasswordAsync();

        Task<bool> PasswordAsync(string password);

        ISeriesSet GetSeriesSet();

        IFlagableSet<Model.JryVideo> GetVideoSet();

        IEntitySet<UserWatchInfo> GetUserWatchInfoSet();

        IFlagSet GetFlagSet();

        IEntitySet<JryCover> GetCoverSet();

        IArtistSet GetArtistSet();

        IVideoRoleCollectionSet GetVideoRoleInfoSet();

        IEntitySet<JrySettingItem> GetSettingSet();
    }
}