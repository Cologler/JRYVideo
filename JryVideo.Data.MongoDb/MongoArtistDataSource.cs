using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;
using System.Collections.Generic;

namespace JryVideo.Data.MongoDb
{
    public sealed class MongoArtistDataSource : MongoJryEntitySet<JryArtist, JryArtist.QueryParameter>, IArtistSet
    {
        public MongoArtistDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JryArtist> collection)
            : base(engine, collection)
        {
        }

        protected override IEnumerable<FilterDefinition<JryArtist>> BuildFilters(JryArtist.QueryParameter parameter)
        {
            if (parameter.DoubanId != null)
                yield return Builders<JryArtist>.Filter.Eq(t => t.DoubanId, parameter.DoubanId);

            if (parameter.TheTVDBId != null)
                yield return Builders<JryArtist>.Filter.Eq(t => t.TheTVDBId, parameter.TheTVDBId);
        }
    }
}