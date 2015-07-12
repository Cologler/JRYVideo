using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoEntitySet<TEntity> : IJasilyEntitySetReader<TEntity, string>, IJasilyEntitySetWriter<TEntity, string>, IJasilyLoggerObject<TEntity>
        where TEntity : class, IJasilyEntity<string>
    {
        public IMongoCollection<TEntity> Collection { get; private set; }

        public MongoEntitySet(IMongoCollection<TEntity> collection)
        {
            this.Collection = collection;
        } 

        /// <summary>
        /// return null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async virtual Task<TEntity> FindAsync(string id)
        {
            var filter = Builders<TEntity>.Filter;

            return (await (await this.Collection.FindAsync(
                filter.Eq(t => t.Id, id)))
                .ToListAsync())
                .FirstOrDefault();
        }

        public async virtual Task<IEnumerable<TEntity>> ListAsync(int skip = 0, int take = Int32.MaxValue)
        {
            var option = new FindOptions<TEntity, TEntity>()
            {
                Skip = skip,
                Limit = take
            };
            var sorter = this.BuildDefaultSort();
            if (sorter != null) option.Sort = sorter;

            return await (await this.Collection.FindAsync(_ => true, option)).ToListAsync();
        }

        protected virtual SortDefinition<TEntity> BuildDefaultSort()
        {
            return null;
        }

        protected void Print(TEntity entity, string @operator)
        {
            this.Log(JasilyLogger.LoggerMode.Release, String.Format("{0} \r\n{1}\r\n", @operator, entity.Print()));
        }

        public async Task<bool> InsertAsync(TEntity entity)
        {
            this.Print(entity, "insert");

            await this.Collection.InsertOneAsync(entity);
            return true;
        }

        public async Task<bool> InsertAsync(IEnumerable<TEntity> items)
        {
            foreach (var item in items)
                this.Print(item, "insert");

            await this.Collection.InsertManyAsync(items);
            return true;
        }

        public async virtual Task<bool> UpdateAsync(TEntity entity)
        {
            this.Print(entity, "update");
            this.Log(JasilyLogger.LoggerMode.Release, "update \r\n" + entity.Print() + "\r\n");
            var filter = Builders<TEntity>.Filter;
            return (await this.Collection.ReplaceOneAsync(
                filter.Eq(t => t.Id, entity.Id),
                entity)).MatchedCount == 1;
        }

        public async Task<bool> RemoveAsync(string id)
        {
            this.Log(JasilyLogger.LoggerMode.Release, "remove \r\n" + id);
            var filter = Builders<TEntity>.Filter;
            return (await this.Collection.DeleteOneAsync(filter.Eq(t => t.Id, id))).DeletedCount == 1;
        }
    }
}