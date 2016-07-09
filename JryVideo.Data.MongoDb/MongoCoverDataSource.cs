using System.Collections.Generic;
using System.Diagnostics;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

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

            if (parameter.ActorId != null)
            {
                if (parameter.VideoId != null)
                {
                    yield return Builders<JryCover>.Filter.And(baseFilter,
                        Builders<JryCover>.Filter.Eq(t => t.ActorId, parameter.ActorId),
                        Builders<JryCover>.Filter.Eq(t => t.VideoId, parameter.VideoId));
                }
                else if (parameter.SeriesId != null)
                {
                    yield return Builders<JryCover>.Filter.And(baseFilter,
                        Builders<JryCover>.Filter.Eq(t => t.ActorId, parameter.ActorId),
                        Builders<JryCover>.Filter.Eq(t => t.SeriesId, parameter.SeriesId));
                }
                else // artist
                {
                    Debug.Assert(parameter.CoverType == CoverType.Artist);
                    yield return Builders<JryCover>.Filter.And(baseFilter,
                        Builders<JryCover>.Filter.Eq(t => t.ActorId, parameter.ActorId));
                }

                yield break;
            }

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