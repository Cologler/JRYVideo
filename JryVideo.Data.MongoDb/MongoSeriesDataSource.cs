using System;
using System.Collections.Generic;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoSeriesDataSource : IDataSourceProvider<JrySeries>
    {
        public IMongoCollection<JrySeries> Collection { get; set; }

        public MongoSeriesDataSource(IMongoCollection<JrySeries> collection)
        {
            this.Collection = collection;
        }

        public IEnumerable<JrySeries> Get(int skip = 0, int take = Int32.MaxValue)
        {
            throw new System.NotImplementedException();
        }

        public void Put(JrySeries value)
        {
            this.Collection.InsertOneAsync(value);
        }
    }
}