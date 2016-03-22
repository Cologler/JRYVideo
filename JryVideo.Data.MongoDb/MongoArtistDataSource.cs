using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;
using System.Collections.Generic;

namespace JryVideo.Data.MongoDb
{
    public sealed class MongoArtistDataSource : MongoJryEntitySet<Artist, Artist.QueryParameter>, IArtistSet
    {
        public MongoArtistDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<Artist> collection)
            : base(engine, collection)
        {
        }

        protected override IEnumerable<FilterDefinition<Artist>> BuildFilters(Artist.QueryParameter parameter)
        {
            if (parameter.DoubanId != null)
                yield return Builders<Artist>.Filter.Eq(t => t.DoubanId, parameter.DoubanId);

            if (parameter.TheTVDBId != null)
                yield return Builders<Artist>.Filter.Eq(t => t.TheTVDBId, parameter.TheTVDBId);
        }
    }
}