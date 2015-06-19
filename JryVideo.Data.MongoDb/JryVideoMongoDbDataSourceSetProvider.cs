using System;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Data.MongoDb
{
    public class JryVideoMongoDbDataSourceSetProvider : IJryVideoDataSourceSetProvider
    {
        public const string DataSourceProviderName = "MongoDb";

        public IDataSourceProvider<JrySeries> GetSeriesDataSourceProvider()
        {
            return new MongoSeriesDataSource();
        }

        public IDataSourceProvider<JryCover> GetCoverDataSourceProvider()
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { return DataSourceProviderName; }
        }
    }
}