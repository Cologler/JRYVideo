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

        public async Task<IEnumerable<JrySeries>> QueryAsync(JrySeries.QueryParameter search, int skip, int take)
        {
            var builder = Builders<JrySeries>.Filter;
            FilterDefinition<JrySeries> filter;

            switch (search.Mode)
            {
                case JrySeries.QueryMode.OriginText:
                    var q1 = PropertySelector<JrySeries>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Names)
                        .ToString();
                    Debug.Assert(q1 == "Videos.Names");
                    filter = builder.Or(
                        builder.Regex(z => z.Names, new BsonRegularExpression(new Regex(Regex.Escape(search.Keyword), RegexOptions.IgnoreCase))),
                        builder.Regex(q1, new BsonRegularExpression(new Regex(Regex.Escape(search.Keyword), RegexOptions.IgnoreCase))));
                    break;

                case JrySeries.QueryMode.SeriesId:
                    var s = await this.FindAsync(search.Keyword);
                    return s == null ? Enumerable.Empty<JrySeries>() : s.IntoArray();

                case JrySeries.QueryMode.VideoId:
                    var q2 = PropertySelector<JrySeries>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q2 == "Videos.Id");
                    filter = builder.Eq(q2, search.Keyword);
                    break;

                case JrySeries.QueryMode.EntityId:
                    var q3 = PropertySelector<Model.JryVideo>.Start()
                        .SelectMany(z => z.Entities)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q3 == "Entities.Id");
                    var it = await this.Engine.VideoCollection.FindAsync(Builders<Model.JryVideo>.Filter.Eq(q3, search.Keyword));
                    var en = (await it.ToListAsync()).FirstOrDefault();
                    if (en == null) return Enumerable.Empty<JrySeries>();
                    var q4 = PropertySelector<JrySeries>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q4 == "Videos.Id");
                    filter = builder.Eq(q4, en.Id);
                    break;

                case JrySeries.QueryMode.DoubanId:
                    var q5 = PropertySelector<JrySeries>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.DoubanId)
                        .ToString();
                    Debug.Assert(q5 == "Videos.DoubanId");
                    filter = builder.Eq(q5, search.Keyword);
                    break;

                case JrySeries.QueryMode.Tag:
                    var q6 = PropertySelector<JrySeries>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Tags)
                        .ToString();
                    Debug.Assert(q6 == "Videos.Tags");
                    filter = builder.Or(
                        builder.Eq(nameof(JrySeries.Tags), search.Keyword),
                        builder.Eq(q6, search.Keyword));
                    break;

                case JrySeries.QueryMode.Any:
                    throw new InvalidOperationException();

                case JrySeries.QueryMode.VideoType:
                    var q7 = PropertySelector<JrySeries>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Type)
                        .ToString();
                    Debug.Assert(q7 == "Videos.Type");
                    filter = builder.Eq(q7, search.Keyword);
                    break;

                case JrySeries.QueryMode.VideoYear:
                    var year = JrySeries.QueryParameter.GetYear(search.Keyword);
                    var q8 = PropertySelector<JrySeries>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Year)
                        .ToString();
                    Debug.Assert(q8 == "Videos.Year");
                    filter = builder.Eq(q8, year);
                    break;

                case JrySeries.QueryMode.ImdbId:
                    var q9 = PropertySelector<JrySeries>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.ImdbId)
                        .ToString();
                    Debug.Assert(q9 == "Videos.ImdbId");
                    filter = builder.Eq(q9, search.Keyword);
                    break;

                case JrySeries.QueryMode.Star:
                    var q10 = PropertySelector<JrySeries>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Star)
                        .ToString();
                    Debug.Assert(q10 == "Videos.Star");
                    var stars = JrySeries.QueryParameter.GetStar(search.Keyword);
                    Debug.Assert(stars.Length > 0);
                    filter = builder.In(q10, stars);
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