using JryVideo.Data;

namespace JryVideo.Core.Managers
{
    public class DataCenter
    {
        public DataCenter(IJryVideoDataEngine dataEngine)
        {
            this.ProviderManager = dataEngine;
            this.CoverManager = new CoverManager(dataEngine.GetCoverSet());
            this.SeriesManager = new SeriesManager(this, dataEngine.GetSeriesSet());
            this.VideoManager = new VideoManager(dataEngine.GetVideoSet());
            this.FlagManager = new FlagManager(dataEngine.GetFlagSet());
            this.ArtistManager = new ArtistManager(dataEngine.GetArtistSet());
            this.VideoRoleManager = new VideoRoleManager(dataEngine.GetVideoRoleInfoSet());

            this.SeriesManager.VideoInfoCreated += this.VideoManager.SeriesManager_VideoInfoCreated;
            this.SeriesManager.VideoInfoRemoved += this.VideoManager.SeriesManager_VideoInfoRemoved;

            this.SeriesManager.VideoInfoCreated += this.FlagManager.SeriesManager_VideoInfoCreated;
            this.SeriesManager.VideoInfoUpdated += this.FlagManager.SeriesManager_VideoInfoUpdated;
            this.SeriesManager.VideoInfoRemoved += this.FlagManager.SeriesManager_VideoInfoRemoved;
            this.VideoManager.EntitiesCreated += this.FlagManager.VideoManager_EntitiesCreated;
            this.VideoManager.EntitiesUpdated += this.FlagManager.VideoManager_EntitiesUpdated;
            this.VideoManager.EntitiesRemoved += this.FlagManager.VideoManager_EntitiesRemoved;

            this.FlagManager.FlagChanged += this.VideoManager.FlagManager_FlagChanged;
        }

        public IJryVideoDataEngine ProviderManager { get; }

        public CoverManager CoverManager { get; }

        public SeriesManager SeriesManager { get; }

        public VideoManager VideoManager { get; }

        public FlagManager FlagManager { get; }

        public ArtistManager ArtistManager { get; }

        public VideoRoleManager VideoRoleManager { get; private set; }
    }
}