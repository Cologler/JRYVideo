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
    public class MongoFlagDataSource : MongoJryEntitySet<JryFlag>, IFlagSet
    {
        private static readonly FindOneAndUpdateOptions<JryFlag, JryFlag> IncrementOptions =
               new FindOneAndUpdateOptions<JryFlag, JryFlag> { IsUpsert = true };

        public MongoFlagDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JryFlag> collection)
            : base(engine, collection)
        {
        }

        public async Task<IEnumerable<JryFlag>> QueryAsync(JryFlagType type)
        {
            var filter = Builders<JryFlag>.Filter;

            return (await (await this.Collection.FindAsync(
                filter.Eq(t => t.Type, type)))
                .ToListAsync())
                .OrderByDescending(z => z.Count);
        }

        public async Task<bool> IncrementAsync(JryFlagType type, string value, int count)
        {
            if (count == 0) return true; // not changed any thing

            var flagName = string.Format("<{0}>[{1}]", type, value);
            this.Log(JasilyLogger.LoggerMode.Debug, string.Format("calc flag {0} => {1}", flagName, count));

            var id = JryFlag.BuildFlagId(type, value);
            var filter = Builders<JryFlag>.Filter;
            var filterWithId = filter.Eq(z => z.Id, id);

            var option = count > 0 ? IncrementOptions : null;
            var flag = await this.Collection.FindOneAndUpdateAsync(
                filterWithId,
                Builders<JryFlag>.Update.Combine(
                    Builders<JryFlag>.Update.Inc(z => z.Count, count),
                    Builders<JryFlag>.Update.Set(z => z.Updated, DateTime.UtcNow)),
                option); // FindOneAndUpdateAsync return old flag.

            if (flag == null) // new flag.
            {
                if (count < 0) // count if flag cannot less then 0.
                {
                    await this.CollectAsync();
                    return true;
                }

                while (true)
                {
                    flag = await this.FindAsync(id);
                    flag.Type = type;
                    flag.Value = value;
                    flag.BuildMetaData(true);
                    Debug.Assert(id == flag.Id);
                    this.BeforeSave(flag);
                    var filterWithCount = filter.And(filterWithId, filter.Eq(z => z.Count, flag.Count));
                    if (await this.Collection.FindOneAndReplaceAsync(filterWithCount, flag) != null) return true;
                }
            }
            else
            {
                if (flag.Count + count < 0) await this.CollectAsync();
            }

            return true;
        }

        private Task CollectAsync()
            => this.Collection.FindOneAndDeleteAsync(Builders<JryFlag>.Filter.Lt(z => z.Count, 1));
    }
}