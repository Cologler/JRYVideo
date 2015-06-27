using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoVideoDataSource : MongoItemDataSource<Model.JryVideo>
    {
        public MongoVideoDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<Model.JryVideo> collection)
            : base(engine, collection)
        {
        }
    }
}