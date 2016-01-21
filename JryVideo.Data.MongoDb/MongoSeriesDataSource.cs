using Jasily.ComponentModel;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JryVideo.Data.MongoDb
{
    public class MongoSeriesDataSource : MongoJryEntitySet<JrySeries>, ISeriesSet
    {
        public MongoSeriesDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<JrySeries> collection)
            : base(engine, collection)
        {
        }

        public async Task<IEnumerable<JrySeries>> QueryAsync(SearchElement search, int skip, int take)
        {
            var builder = Builders<JrySeries>.Filter;
            FilterDefinition<JrySeries> filter;

            switch (search.Type)
            {
                case SearchElement.ElementType.Text:
                    var q1 = PropertySelector<JrySeries>.Start(z => z)
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Names)
                        .ToString();
                    Debug.Assert(q1 == "Videos.Names");
                    filter = builder.Or(
                        builder.Regex(z => z.Names, new BsonRegularExpression(new Regex(Regex.Escape(search.Value), RegexOptions.IgnoreCase))),
                        builder.Regex(q1, new BsonRegularExpression(new Regex(Regex.Escape(search.Value), RegexOptions.IgnoreCase))));
                    break;

                case SearchElement.ElementType.SeriesId:
                    var s = await this.FindAsync(search.Value);
                    return s == null ? Enumerable.Empty<JrySeries>() : s.IntoArray();

                case SearchElement.ElementType.VideoId:
                    var q2 = PropertySelector<JrySeries>.Start(z => z)
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q2 == "Videos.Id");
                    filter = builder.Eq(q2, search.Value);
                    break;

                case SearchElement.ElementType.EntityId:
                    var q3 = PropertySelector<Model.JryVideo>.Start(z => z)
                        .SelectMany(z => z.Entities)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q3 == "Entities.Id");
                    var it = await this.Engine.VideoCollection.FindAsync(Builders<Model.JryVideo>.Filter.Eq(q3, search.Value));
                    var en = (await it.ToListAsync()).FirstOrDefault();
                    if (en == null) return Enumerable.Empty<JrySeries>();
                    var q4 = PropertySelector<JrySeries>.Start(z => z)
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q4 == "Videos.Id");
                    filter = builder.Eq(q4, en.Id);
                    break;

                case SearchElement.ElementType.DoubanId:
                    var q5 = PropertySelector<JrySeries>.Start(z => z)
                        .SelectMany(z => z.Videos)
                        .Select(z => z.DoubanId)
                        .ToString();
                    Debug.Assert(q5 == "Videos.DoubanId");
                    filter = builder.Eq(q5, search.Value);
                    break;

                case SearchElement.ElementType.Tag:
                    var q6 = PropertySelector<JrySeries>.Start(z => z)
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Tags)
                        .ToString();
                    Debug.Assert(q6 == "Videos.Tags");
                    filter = builder.Or(
                        builder.Eq(nameof(JrySeries.Tags), search.Value),
                        builder.Eq(q6, search.Value));
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

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

        protected override SortDefinition<JrySeries> BuildDefaultSort()
        {
            var sort = PropertySelector<JrySeries>.Start(z => z)
                .SelectMany(z => z.Videos)
                .Select(z => z.Created)
                .ToString();
            Debug.Assert(sort == "Videos.Created");
            return Builders<JrySeries>.Sort.Descending(sort);
        }

        public async Task<IEnumerable<JrySeries>> ListTrackingAsync()
        {
            var builder = Builders<JrySeries>.Filter;

            var filter = builder.Eq("Videos.IsTracking", true);

            return await (await this.Collection.FindAsync(
                filter,
                options: new FindOptions<JrySeries, JrySeries>()
                {
                    Sort = this.BuildDefaultSort()
                }))
                .ToListAsync();
        }
    }
}