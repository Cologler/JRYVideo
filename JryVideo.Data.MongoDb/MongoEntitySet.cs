using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model.Interfaces;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoEntitySet<TEntity> : IEntitySet<TEntity>, IJasilyLoggerObject<TEntity>
        where TEntity : class, IObject
    {
        internal IMongoCollection<TEntity> Collection { get; }

        public JryVideoMongoDbDataEngine Engine { get; }

        public MongoEntitySet(JryVideoMongoDbDataEngine engine, IMongoCollection<TEntity> collection)
        {
            this.Engine = engine;
            this.Collection = collection;
        }

        public async Task CursorAsync(Action<TEntity> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            this.Engine.TestPass();
            using (var cur = await this.Collection.FindAsync(_ => true))
            {
                while (await cur.MoveNextAsync())
                {
                    foreach (var obj in cur.Current)
                    {
                        callback(obj);
                    }
                }
            }
        }

        public async Task CursorAsync(Expression<Func<TEntity, bool>> filter, Action<TEntity> callback)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            this.Engine.TestPass();
            using (var cur = await this.Collection.FindAsync(filter))
            {
                while (await cur.MoveNextAsync())
                {
                    foreach (var obj in cur.Current)
                    {
                        callback(obj);
                    }
                }
            }
        }

        public async Task NotExistsFieldCursorAsync(string field, Action<TEntity> callback)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            this.Engine.TestPass();
            var filter = Builders<TEntity>.Filter.Exists(field, false);
            using (var cur = await this.Collection.FindAsync(filter))
            {
                while (await cur.MoveNextAsync())
                {
                    foreach (var obj in cur.Current)
                    {
                        callback(obj);
                    }
                }
            }
        }

        /// <summary>
        /// return null if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> FindAsync(string id)
        {
            this.Engine.TestPass();
            return (await (await this.Collection.FindAsync(
                Builders<TEntity>.Filter.Eq(t => t.Id, id)))
                .ToListAsync())
                .FirstOrDefault();
        }

        /// <summary>
        /// return a entities dictionary where match id.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public virtual async Task<IDictionary<string, TEntity>> FindAsync(IEnumerable<string> ids)
        {
            this.Engine.TestPass();
            return (await (await this.Collection.FindAsync(
                Builders<TEntity>.Filter.In(t => t.Id, ids.ToArray())))
                .ToListAsync())
                .ToDictionary(z => z.Id);
        }

        public virtual async Task<IEnumerable<TEntity>> ListAsync(int skip = 0, int take = int.MaxValue)
        {
            this.Engine.TestPass();
            var option = new FindOptions<TEntity, TEntity>()
            {
                Skip = skip,
                Limit = take
            };
            option.Sort = this.BuildDefaultSort() ?? option.Sort;
            return await (await this.Collection.FindAsync(_ => true, option)).ToListAsync();
        }

        protected virtual SortDefinition<TEntity> BuildDefaultSort() => null;

        protected void Print(TEntity entity, string @operator)
            => this.Log(JasilyLogger.LoggerMode.Release, $"{@operator} \r\n{entity.Print()}\r\n");

        public async Task<bool> InsertAsync(TEntity entity)
        {
            this.Engine.TestPass();
            this.Print(entity, "insert");
            this.BeforeSave(entity);
            await this.Collection.InsertOneAsync(entity);
            return true;
        }

        public async Task<bool> InsertAsync(IEnumerable<TEntity> items)
        {
            this.Engine.TestPass();
            var entities = items as TEntity[] ?? items.ToArray();
            foreach (var item in entities)
            {
                this.Print(item, "insert");
                this.BeforeSave(item);
            }
            await this.Collection.InsertManyAsync(entities);
            return true;
        }

        public async Task<bool> InsertOrUpdateAsync(TEntity entity)
        {
            this.Engine.TestPass();
            this.Print(entity, "insertOrUpdate");
            this.BeforeSave(entity);
            await this.Collection.FindOneAndReplaceAsync(
                Builders<TEntity>.Filter.Eq(z => z.Id, entity.Id),
                entity,
                new FindOneAndReplaceOptions<TEntity, TEntity>() { IsUpsert = true });
            return true;
        }

        public virtual async Task<bool> UpdateAsync(TEntity entity)
        {
            this.Engine.TestPass();
            this.Print(entity, "update");
            this.BeforeSave(entity);
            this.Log(JasilyLogger.LoggerMode.Release, "update \r\n" + entity.Print() + "\r\n");
            var filter = Builders<TEntity>.Filter;
            return (await this.Collection.ReplaceOneAsync(
                filter.Eq(t => t.Id, entity.Id),
                entity)).MatchedCount == 1;
        }

        public async Task<bool> RemoveAsync(string id)
        {
            this.Engine.TestPass();
            this.Log(JasilyLogger.LoggerMode.Release, "remove \r\n" + id);
            var filter = Builders<TEntity>.Filter;
            return (await this.Collection.DeleteOneAsync(filter.Eq(t => t.Id, id))).DeletedCount == 1;
        }

        protected void BeforeSave(TEntity obj)
        {
            obj.CheckError();
            var updated = obj as IUpdated;
            if (updated != null) updated.Updated = DateTime.UtcNow;
        }
    }
}