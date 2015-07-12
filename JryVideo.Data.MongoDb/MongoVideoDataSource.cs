using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoVideoDataSource : MongoJryEntitySet<Model.JryVideo>
    {
        public MongoVideoDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<Model.JryVideo> collection)
            : base(engine, collection)
        {
        }
    }
}