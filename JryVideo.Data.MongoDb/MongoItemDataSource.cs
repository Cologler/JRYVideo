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
        public JryVideoMongoDbDataEngine Engine { get; private set; }

        public IMongoCollection<T> Collection { get; private set; }

        public MongoItemDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<T> collection)
        {
            this.Engine = engine;
            this.Collection = collection;
        }

        public async virtual Task<IEnumerable<T>> QueryAsync(int skip = 0, int take = Int32.MaxValue)
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

        public async virtual Task<T> QueryAsync(string id)
        {
            var filter = Builders<T>.Filter;

            return (await (await this.Collection.FindAsync(
                filter.Eq(t => t.Id, id)))
                .ToListAsync())
                .FirstOrDefault();
        }

        public async virtual Task<bool> InsertAsync(T value)
        {
            await this.Collection.InsertOneAsync(value);
            return true;
        }

        public async virtual Task<bool> UpdateAsync(T value)
        {
            var filter = Builders<T>.Filter;

            return (await this.Collection.ReplaceOneAsync(
                    filter.Eq(t => t.Id, value.Id),
                    value)).MatchedCount == 1;
        }

        public async Task<bool> RemoveAsync(T value)
        {
            var filter = Builders<T>.Filter;

            return (await this.Collection.DeleteOneAsync(
                    filter.Eq(t => t.Id, value.Id))).DeletedCount == 1;
        }
    }
}