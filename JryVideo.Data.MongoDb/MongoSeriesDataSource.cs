using System.Linq;
using System.Threading.Tasks;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoSeriesDataSource : MongoItemDataSource<JrySeries>
    {
        public MongoSeriesDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JrySeries> collection)
            : base(engine, collection)
        {
        }

        public override async Task<bool> InsertAsync(JrySeries value)
        {
            if (await base.InsertAsync(value))
            {
                var counterDataSource = this.Engine.GetCounterDataSourceProvider();

                if (value.Videos != null)
                {
                    foreach (var year in value.Videos.Select(z => z.Year.ToString()))
                    {
                        await counterDataSource.RefAddOneAsync(JryCounterType.Year, year);
                    }
                }

                

                return true;
            }

            return false;
        }
    }
}