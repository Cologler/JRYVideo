using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoFlagDataSource : MongoItemDataSource<JryFlag>, IFlagDataSourceProvider
    {
        public MongoFlagDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JryFlag> collection)
            : base(engine, collection)
        {
        }

        public async Task<IEnumerable<JryFlag>> QueryAsync(JryFlagType type)
        {
            var filter = Builders<JryFlag>.Filter;

            return await (await this.Collection.FindAsync(
                filter.Eq(t => t.Type, type)))
                .ToListAsync();
        }

        public async Task<bool> RefMathAsync(JryFlagType type, string value, int count)
        {
            if (count == 0) return true;

            var id = JryFlag.BuildCounterId(type, value);
            var filter = Builders<JryFlag>.Filter;
            var update = Builders<JryFlag>.Update;

            var flag = await this.Collection.FindOneAndUpdateAsync(
                filter.Eq(z => z.Id, id),
                update.Inc(z => z.Count, count),
                new FindOneAndUpdateOptions<JryFlag, JryFlag>() { IsUpsert = true });

            if (flag == null)
            {
                if (count < 1) return true;

                flag = await this.FindAsync(id);
                flag.Type = type;
                flag.Value = value;
                flag.BuildMetaData(true);
                return await this.UpdateAsync(flag);
            }
            else
            {
                if (flag.Count + count < 0)
                {
                    await this.Collection.FindOneAndDeleteAsync(filter.Lt(z => z.Count, 0));
                }
            }

            return true;
        }
    }
}