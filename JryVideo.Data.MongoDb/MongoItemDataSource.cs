using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoItemDataSource<T> : IDataSourceProvider<T>
        where T : JryObject
    {
        public IMongoCollection<T> Collection { get; set; }

        public MongoItemDataSource(IMongoCollection<T> collection)
        {
            this.Collection = collection;
        }

        public async Task<IEnumerable<T>> QueryAsync(int skip = 0, int take = Int32.MaxValue)
        {
            return await (await this.Collection.FindAsync(
                _ => true,
                options: new FindOptions<T, T>()
                {
                    Skip = skip,
                    Limit = take
                }))
                .ToListAsync();
        }

        public async Task<T> QueryAsync(Guid id)
        {
            var filter = Builders<T>.Filter;

            return (await (await this.Collection.FindAsync(
                filter.Eq(t => t.Id, id)))
                .ToListAsync())
                .FirstOrDefault();
        }

        public async Task<bool> InsertAsync(T value)
        {
            await this.Collection.InsertOneAsync(value);
            return true;
        }

        public async Task<bool> UpdateAsync(T value)
        {
            var filter = Builders<T>.Filter;

            return
                (await this.Collection.ReplaceOneAsync(
                    filter.Eq(t => t.Id, value.Id),
                    value)).MatchedCount == 1;
        }
    }
}