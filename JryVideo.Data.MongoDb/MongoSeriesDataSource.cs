using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoSeriesDataSource : MongoItemDataSource<JrySeries>//, IDataSourceProvider<JrySeries>
    {
        public MongoSeriesDataSource(IMongoCollection<JrySeries> collection)
            : base(collection)
        {
        }
    }
}