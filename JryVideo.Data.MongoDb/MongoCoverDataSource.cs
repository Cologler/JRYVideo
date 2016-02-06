using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;
using System.Collections.Generic;

namespace JryVideo.Data.MongoDb
{
    public class MongoCoverDataSource : MongoJryEntitySet<JryCover, JryCover.QueryParameter>, ICoverSet
    {
        public MongoCoverDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JryCover> collection)
            : base(engine, collection)
        {
        }

        protected override IEnumerable<FilterDefinition<JryCover>> BuildFilters(JryCover.QueryParameter parameter)
        {
            var baseFilter = Builders<JryCover>.Filter.Eq(t => t.CoverType, parameter.CoverType);

            if (parameter.VideoId != null)
            {
                yield return Builders<JryCover>.Filter.And(baseFilter,
                    Builders<JryCover>.Filter.Eq(t => t.VideoId, parameter.VideoId));
            }
            else if (parameter.SeriesId != null)
            {
                yield return Builders<JryCover>.Filter.And(baseFilter,
                    Builders<JryCover>.Filter.Eq(t => t.SeriesId, parameter.SeriesId));
            }
        }
    }
}