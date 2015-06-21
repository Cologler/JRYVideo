using System;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoCounterDataSource : MongoItemDataSource<JryCounter>, ICounterDataSourceProvider
    {
        public MongoCounterDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JryCounter> collection)
            : base(engine, collection)
        {
        }

        public async Task<bool> RefAddAsync(JryCounterType type, string value, int count)
        {
            var item = await this.QueryAsync(JryCounter.BuildCounterId(type, value));

            if (item == null)
            {
                item = new JryCounter()
                {
                    Type = type,
                    Value = value,
                    Count = count
                }.InitializeInstance();

                return await this.InsertAsync(item);
            }
            else
            {
                item.Count += count;

                return await this.UpdateAsync(item);
            }
        }

        public async Task<bool> RefAddOneAsync(JryCounterType type, string value)
        {
            return await this.RefAddAsync(type, value, 1);
        }

        public async Task<bool> RefSubAsync(JryCounterType type, string value, int count)
        {
            var item = await this.QueryAsync(JryCounter.BuildCounterId(type, value));

            if (item == null)
            {
                return false;
            }
            else
            {
                item.Count -= count;

                return item.Count > 0 ? await this.UpdateAsync(item) : await this.RemoveAsync(item);
            }
        }

        public async Task<bool> RefSubOneAsync(JryCounterType type, string value)
        {
            return await this.RefSubAsync(type, value, 1);
        }
    }
}