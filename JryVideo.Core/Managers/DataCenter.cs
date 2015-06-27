using JryVideo.Data;

namespace JryVideo.Core.Managers
{
    public class DataCenter
    {
        public DataCenter(IJryVideoDataEngine dataEngine)
        {
            this.ProviderManager = dataEngine;
            this.CoverManager = new CoverManager(dataEngine.GetCoverDataSourceProvider());
            this.SeriesManager = new SeriesManager(this, dataEngine.GetSeriesDataSourceProvider());
            this.VideoManager = new VideoManager(dataEngine.GetVideoDataSourceProvider());
            this.FlagManager = new FlagManager(dataEngine.GetCounterDataSourceProvider());
            this.ArtistManager = new ArtistManager(dataEngine.GetArtistDataSourceProvider());

            this.SeriesManager.VideoInfoCreated += this.VideoManager.SeriesManager_VideoInfoCreated;
            this.SeriesManager.VideoInfoRemoved += this.VideoManager.SeriesManager_VideoInfoRemoved;

            this.SeriesManager.VideoInfoCreated += this.FlagManager.SeriesManager_VideoInfoCreated;
            this.SeriesManager.VideoInfoRemoved += this.FlagManager.SeriesManager_VideoInfoRemoved;
        }

        public IJryVideoDataEngine ProviderManager { get; private set; }

        public CoverManager CoverManager { get; private set; }

        public SeriesManager SeriesManager { get; private set; }

        public VideoManager VideoManager { get; private set; }

        public FlagManager FlagManager { get; private set; }

        public ArtistManager ArtistManager { get; private set; }
    }
}