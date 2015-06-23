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
            this.CounterManager = new CounterManager(dataEngine.GetCounterDataSourceProvider());
            this.ArtistManager = new ArtistManager(dataEngine.GetArtistDataSourceProvider());
        }

        public IJryVideoDataEngine ProviderManager { get; private set; }

        public CoverManager CoverManager { get; private set; }

        public SeriesManager SeriesManager { get; private set; }

        public CounterManager CounterManager { get; private set; }

        public ArtistManager ArtistManager { get; private set; }
    }
}