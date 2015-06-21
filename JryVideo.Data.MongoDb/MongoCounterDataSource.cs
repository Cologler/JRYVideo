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

        public async Task<bool> RefMathAsync(JryCounterType type, string value, int count)
        {
            if (count == 0) return true;

            var item = await this.QueryAsync(JryCounter.BuildCounterId(type, value));

            if (item == null)
            {
                if (count > 0)
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
                    return false;
                }
            }
            else
            {
                item.Count += count;

                return item.Count > 0 ? await this.UpdateAsync(item) : await this.RemoveAsync(item);
            }
        }
    }
}