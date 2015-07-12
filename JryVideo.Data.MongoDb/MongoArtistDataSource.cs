using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoArtistDataSource : MongoJryEntitySet<JryArtist>
    {
        public MongoArtistDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JryArtist> collection)
            : base(engine, collection)
        {
        }
    }
}