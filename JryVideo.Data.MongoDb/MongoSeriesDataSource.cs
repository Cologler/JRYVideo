using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoSeriesDataSource : MongoItemDataSource<JrySeries>, ISeriesDataSourceProvider
    {
        public MongoSeriesDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JrySeries> collection)
            : base(engine, collection)
        {
        }

        public async virtual Task<IEnumerable<JrySeries>> QueryByNameAsync(string searchText,
            int skip = 0, int take = Int32.MaxValue)
        {
            var builder = Builders<JrySeries>.Filter;

            var filter = builder.Or(
                builder.Regex(z => z.Names, new BsonRegularExpression(new Regex(Regex.Escape(searchText)))),
                builder.Regex("Videos.Names", new BsonRegularExpression(new Regex(Regex.Escape(searchText)))));

            return await (await this.Collection.FindAsync(
                filter,
                options: new FindOptions<JrySeries, JrySeries>()
                {
                    Skip = skip,
                    Limit = take,
                    Sort = this.BuildDefaultSort()
                }))
                .ToListAsync();
        }
    }
}