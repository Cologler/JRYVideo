using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoItemDataSource<T> : IDataSourceProvider<T>, IJasilyLoggerObject<T>
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
                    Limit = take,
                    Sort = this.BuildDefaultSort()
                }))
                .ToListAsync();
        }

        protected virtual SortDefinition<T> BuildDefaultSort()
        {
            var sorter = Builders<T>.Sort;

            return sorter.Descending(z => z.Created);
        }

        public async virtual Task<T> FindAsync(string id)
        {
            var filter = Builders<T>.Filter;

            return (await (await this.Collection.FindAsync(
                filter.Eq(t => t.Id, id)))
                .ToListAsync())
                .FirstOrDefault();
        }

        public async virtual Task<bool> InsertAsync(T entity)
        {
            this.Log(JasilyLogger.LoggerMode.Release, "insert \r\n" + entity.Print() + "\r\n");
            await this.Collection.InsertOneAsync(entity);
            return true;
        }

        public async Task<bool> InsertAsync(IEnumerable<T> items)
        {
            foreach (var item in items)
                this.Log(JasilyLogger.LoggerMode.Release, "insert \r\n" + item.Print() + "\r\n");
            await this.Collection.InsertManyAsync(items);
            return true;
        }

        public async virtual Task<bool> UpdateAsync(T entity)
        {
            this.Log(JasilyLogger.LoggerMode.Release, "update \r\n" + entity.Print() + "\r\n");
            var filter = Builders<T>.Filter;
            return (await this.Collection.ReplaceOneAsync(
                    filter.Eq(t => t.Id, entity.Id),
                    entity)).MatchedCount == 1;
        }

        public async Task<bool> RemoveAsync(string id)
        {
            this.Log(JasilyLogger.LoggerMode.Release, "remove \r\n" + id);
            var filter = Builders<T>.Filter;
            return (await this.Collection.DeleteOneAsync(filter.Eq(t => t.Id, id))).DeletedCount == 1;
        }
    }
}