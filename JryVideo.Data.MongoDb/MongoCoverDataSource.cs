using System;
using System.Collections.Generic;
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
            => Builders<JryCover>.Filter.Eq(t => t.Id, parameter.Id).IntoArray();
    }
}