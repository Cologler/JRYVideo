using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoCoverDataSource : MongoJryEntitySet<JryCover>, ICoverSet
    {
        public MongoCoverDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JryCover> collection)
            : base(engine, collection)
        {
        }

        public async Task<IEnumerable<JryCover>> QueryByDoubanIdAsync(JryCoverType coverType, string doubanId)
        {
            var filter = Builders<JryCover>.Filter;

            return await (await this.Collection.FindAsync(
                filter.And(
                    filter.Eq(t => t.CoverType, coverType),
                    filter.Eq(t => t.DoubanId, doubanId))
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<JryCover>> QueryByUriAsync(JryCoverType coverType, string uri)
        {
            var filter = Builders<JryCover>.Filter;

            return await (await this.Collection.FindAsync(
                filter.And(
                    filter.Eq(t => t.CoverType, coverType),
                    filter.Eq(t => t.Uri, uri))
                ))
                .ToListAsync();
        }
    }
}