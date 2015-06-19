using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoArtistDataSource : MongoItemDataSource<JryArtist>
    {
        public MongoArtistDataSource(IMongoCollection<JryArtist> collection)
            : base(collection)
        {
        }
    }
}