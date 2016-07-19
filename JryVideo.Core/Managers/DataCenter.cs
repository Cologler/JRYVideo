using System.Threading.Tasks;
using JryVideo.Core.Managers.Journals;
using JryVideo.Data;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class DataCenter
    {
        public static readonly DataCenter NotWork = new DataCenter(false);

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
            this.UserWatchInfoManager = new UserWatchInfoManager(dataEngine.GetUserWatchInfoSet());

            // initialize
            this.SeriesManager.Initialize(this);
            this.VideoManager.Initialize(this);
            this.FlagManager.Initialize(this);
            this.VideoRoleManager.Initialize(this);
            this.CoverManager.Initialize(this);

            this.Journal.Initialize(this);
        }

        public IJryVideoDataEngine ProviderManager { get; }

        public CoverManager CoverManager { get; }

        public SeriesManager SeriesManager { get; }

        public VideoManager VideoManager { get; }

        public FlagManager FlagManager { get; }

        public ArtistManager ArtistManager { get; }

        public VideoRoleManager VideoRoleManager { get; }

        public UserWatchInfoManager UserWatchInfoManager { get; }

        public DataJournal Journal { get; } = new DataJournal();

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

        public async Task<CombineResult> CanCombineAsync(Series to, Series from)
        {
            var result = await this.SeriesManager.CanCombineAsync(to.Id, from.Id);
            if (!result.CanCombine) return result;
            result = await this.VideoRoleManager.CanCombineAsync(to.Id, from.Id);
            if (!result.CanCombine) return result;
            return CombineResult.True;
        }

        public async Task<CombineResult> CombineAsync(Series to, Series from)
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