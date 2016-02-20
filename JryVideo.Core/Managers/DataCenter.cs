using JryVideo.Data;
using JryVideo.Model;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public class DataCenter
    {
        public static DataCenter NotWork = new DataCenter(false);

        public bool IsWork { get; }

        private DataCenter(bool isWork)
        {
            this.IsWork = isWork;
        }

        public DataCenter(IJryVideoDataEngine dataEngine)
            : this(true)
        {
            this.ProviderManager = dataEngine;
            this.CoverManager = new CoverManager(dataEngine.GetCoverSet());
            this.SeriesManager = new SeriesManager(this, dataEngine.GetSeriesSet());
            this.VideoManager = new VideoManager(dataEngine.GetVideoSet());
            this.FlagManager = new FlagManager(dataEngine.GetFlagSet());
            this.ArtistManager = new ArtistManager(dataEngine.GetArtistSet());
            this.VideoRoleManager = new VideoRoleManager(this.SeriesManager, this.ArtistManager, dataEngine.GetVideoRoleInfoSet());

            // video
            this.SeriesManager.VideoInfoRemoved += this.VideoManager.SeriesManager_VideoInfoRemoved;
            this.FlagManager.FlagChanged += this.VideoManager.FlagManager_FlagChanged;

            // flag
            this.SeriesManager.VideoInfoCreated += this.FlagManager.SeriesManager_VideoInfoCreated;
            this.SeriesManager.VideoInfoUpdated += this.FlagManager.SeriesManager_VideoInfoUpdated;
            this.SeriesManager.VideoInfoRemoved += this.FlagManager.SeriesManager_VideoInfoRemoved;
            this.VideoManager.EntitiesCreated += this.FlagManager.VideoManager_EntitiesCreated;
            this.VideoManager.EntitiesUpdated += this.FlagManager.VideoManager_EntitiesUpdated;
            this.VideoManager.EntitiesRemoved += this.FlagManager.VideoManager_EntitiesRemoved;

            // cover
            this.SeriesManager.CoverParentRemoving += this.CoverManager.Manager_CoverParentRemoving;
            this.VideoRoleManager.CoverParentRemoving += this.CoverManager.Manager_CoverParentRemoving;

            // role
            this.SeriesManager.ItemRemoved += this.VideoRoleManager.SeriesManager_ItemRemoved;
            this.SeriesManager.VideoInfoRemoved += this.VideoRoleManager.SeriesManager_VideoInfoRemoved;
        }

        public IJryVideoDataEngine ProviderManager { get; }

        public CoverManager CoverManager { get; }

        public SeriesManager SeriesManager { get; }

        public VideoManager VideoManager { get; }

        public FlagManager FlagManager { get; }

        public ArtistManager ArtistManager { get; }

        public VideoRoleManager VideoRoleManager { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="to">after combine will live</param>
        /// <param name="from">after combine will dead</param>
        /// <returns></returns>
        public async Task<CombineResult> CanCombineAsync(VideoInfoManager manager, JryVideoInfo to, JryVideoInfo from)
        {
            var result = await manager.CanCombineAsync(to.Id, from.Id);
            if (!result.CanCombine) return result;
            result = await this.VideoRoleManager.CanCombineAsync(to.Id, from.Id);
            if (!result.CanCombine) return result;
            return CombineResult.True;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="to">after combine will live</param>
        /// <param name="from">after combine will dead</param>
        /// <returns></returns>
        public async Task<CombineResult> CombineAsync(VideoInfoManager manager, JryVideoInfo to, JryVideoInfo from)
        {
            var result = await this.CanCombineAsync(manager, to, from);
            if (result.CanCombine)
            {
                await this.VideoRoleManager.CombineAsync(to.Id, from.Id);
                await manager.CombineAsync(to, from);
            }
            return result;
        }

        public async Task<CombineResult> CanCombineAsync(JrySeries to, JrySeries from)
        {
            var result = await this.SeriesManager.CanCombineAsync(to.Id, from.Id);
            if (!result.CanCombine) return result;
            result = await this.VideoRoleManager.CanCombineAsync(to.Id, from.Id);
            if (!result.CanCombine) return result;
            return CombineResult.True;
        }

        public async Task<CombineResult> CombineAsync(JrySeries to, JrySeries from)
        {
            var result = await this.CanCombineAsync(to, from);
            if (result.CanCombine)
            {
                await this.VideoRoleManager.CombineAsync(to.Id, from.Id);
                await this.SeriesManager.CombineAsync(to, from);
            }
            return result;
        }
    }
}