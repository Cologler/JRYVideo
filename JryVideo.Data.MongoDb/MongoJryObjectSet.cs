using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using JryVideo.Model.Interfaces;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoJryEntitySet<T> : MongoEntitySet<T>
        where T : JryObject, IObject
    {
        protected MongoJryEntitySet(JryVideoMongoDbDataEngine engine, IMongoCollection<T> collection)
            : base(engine, collection)
        {
        }

        protected override SortDefinition<T> BuildDefaultSort() => Builders<T>.Sort.Descending(z => z.Created);
    }

    public abstract class MongoJryEntitySet<T, TFilterParameters> : MongoJryEntitySet<T>, IQueryableEntitySet<T, TFilterParameters>
        where T : JryObject, IObject, IQueryBy<TFilterParameters>
    {
        protected MongoJryEntitySet(JryVideoMongoDbDataEngine engine, IMongoCollection<T> collection)
            : base(engine, collection)
        {
        }

        public Task<IEnumerable<T>> FindAsync(TFilterParameters parameter)
            => this.QueryAsync(parameter, 0, int.MaxValue);

        public virtual async Task<IEnumerable<T>> QueryAsync(TFilterParameters parameter, int skip, int take)
        {
            var filters = this.BuildFilters(parameter).ToArray();

            if (filters.Length == 0) return Enumerable.Empty<T>();

            var filter = filters.Length == 1
                ? filters[0]
                : filters.Skip(1).Aggregate(filters[0], (current, f) => Builders<T>.Filter.Or(current, f));

            return await (await this.Collection.FindAsync(filter, new FindOptions<T, T>
            {
                Skip = skip,
                Limit = take,
                Sort = this.BuildDefaultSort()
            })).ToListAsync();
        }

        protected abstract IEnumerable<FilterDefinition<T>> BuildFilters(TFilterParameters parameter);
    }
}