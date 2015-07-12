using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoJryEntitySet<T> : MongoEntitySet<T>, IJasilyEntitySetProvider<T, string>
        where T : JryObject
    {
        public JryVideoMongoDbDataEngine Engine { get; private set; }

        public MongoJryEntitySet(JryVideoMongoDbDataEngine engine, IMongoCollection<T> collection)
            : base(collection)
        {
            this.Engine = engine;
        }

        protected override SortDefinition<T> BuildDefaultSort()
        {
            return Builders<T>.Sort.Descending(z => z.Created);
        }
    }
}