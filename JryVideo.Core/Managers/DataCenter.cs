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
        }

        public IJryVideoDataEngine ProviderManager { get; private set; }

        public CoverManager CoverManager { get; private set; }

        public SeriesManager SeriesManager { get; private set; } 
    }
}