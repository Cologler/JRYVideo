using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Model;
using JryVideo.Model.Interfaces;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoJryEntitySet<T> : MongoEntitySet<T>
        where T : JryObject, IObject
    {
        public MongoJryEntitySet(JryVideoMongoDbDataEngine engine, IMongoCollection<T> collection)
            : base(engine, collection)
        {
        }

        protected override SortDefinition<T> BuildDefaultSort()
        {
            return Builders<T>.Sort.Descending(z => z.Created);
        }
    }

    public abstract class MongoJryEntitySet<T, TFilterParameters> : MongoJryEntitySet<T>
        where T : JryObject, IObject
    {
        protected MongoJryEntitySet(JryVideoMongoDbDataEngine engine, IMongoCollection<T> collection)
            : base(engine, collection)
        {
        }

        public async Task<IEnumerable<T>> FindAsync(TFilterParameters parameter)
        {
            var filters = this.BuildFilters(parameter).ToArray();
            if (filters.Length == 0) return Enumerable.Empty<T>();
            var filter = filters.Skip(1).Aggregate(filters[0], (current, f) => Builders<T>.Filter.Or(current, f));

            return await (await this.Collection.FindAsync(filter)).ToListAsync();
        }

        protected abstract IEnumerable<FilterDefinition<T>> BuildFilters(TFilterParameters parameter);
    }
}