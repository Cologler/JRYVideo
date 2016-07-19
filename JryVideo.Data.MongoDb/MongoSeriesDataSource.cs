using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jasily.ComponentModel;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public class MongoSeriesDataSource : MongoJryEntitySet<Series>, ISeriesSet
    {
        public MongoSeriesDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<Series> collection)
            : base(engine, collection)
        {
        }

        public async Task<IEnumerable<Series>> QueryAsync(Series.QueryParameter search, int skip, int take)
        {
            var builder = Builders<Series>.Filter;
            FilterDefinition<Series> filter;

            switch (search.Mode)
            {
                case Series.QueryMode.OriginText:
                    var q1 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Names)
                        .ToString();
                    Debug.Assert(q1 == "Videos.Names");
                    filter = builder.Or(
                        builder.Regex(z => z.Names, new BsonRegularExpression(new Regex(Regex.Escape(search.Keyword), RegexOptions.IgnoreCase))),
                        builder.Regex(q1, new BsonRegularExpression(new Regex(Regex.Escape(search.Keyword), RegexOptions.IgnoreCase))));
                    break;

                case Series.QueryMode.SeriesId:
                    var s = await this.FindAsync(search.Keyword);
                    return s == null ? Enumerable.Empty<Series>() : s.IntoArray();

                case Series.QueryMode.VideoId:
                    var q2 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q2 == "Videos.Id");
                    filter = builder.Eq(q2, search.Keyword);
                    break;

                case Series.QueryMode.EntityId:
                    var q3 = PropertySelector<Model.JryVideo>.Start()
                        .SelectMany(z => z.Entities)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q3 == "Entities.Id");
                    var it = await this.Engine.VideoCollection.FindAsync(Builders<Model.JryVideo>.Filter.Eq(q3, search.Keyword));
                    var en = (await it.ToListAsync()).FirstOrDefault();
                    if (en == null) return Enumerable.Empty<Series>();
                    var q4 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q4 == "Videos.Id");
                    filter = builder.Eq(q4, en.Id);
                    break;

                case Series.QueryMode.DoubanId:
                    var q5 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.DoubanId)
                        .ToString();
                    Debug.Assert(q5 == "Videos.DoubanId");
                    filter = builder.Eq(q5, search.Keyword);
                    break;

                case Series.QueryMode.Tag:
                    var q6 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Tags)
                        .ToString();
                    Debug.Assert(q6 == "Videos.Tags");
                    filter = builder.Or(
                        builder.Eq(nameof(Series.Tags), search.Keyword),
                        builder.Eq(q6, search.Keyword));
                    break;

                case Series.QueryMode.Any:
                    throw new InvalidOperationException();

                case Series.QueryMode.VideoType:
                    var q7 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Type)
                        .ToString();
                    Debug.Assert(q7 == "Videos.Type");
                    filter = builder.Eq(q7, search.Keyword);
                    break;

                case Series.QueryMode.VideoYear:
                    var year = Series.QueryParameter.GetYear(search.Keyword);
                    var q8 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Year)
                        .ToString();
                    Debug.Assert(q8 == "Videos.Year");
                    filter = builder.Eq(q8, year);
                    break;

                case Series.QueryMode.ImdbId:
                    var q9 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.ImdbId)
                        .ToString();
                    Debug.Assert(q9 == "Videos.ImdbId");
                    filter = builder.Eq(q9, search.Keyword);
                    break;

                case Series.QueryMode.Star:
                    var q10 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Star)
                        .ToString();
                    Debug.Assert(q10 == "Videos.Star");
                    var stars = Series.QueryParameter.GetStar(search.Keyword);
                    Debug.Assert(stars.Length > 0);
                    filter = builder.In(q10, stars);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return await (await this.Collection.FindAsync(
                filter,
                options: new FindOptions<Series, Series>()
                {
                    Skip = skip,
                    Limit = take,
                    Sort = this.BuildDefaultSort()
                }))
                .ToListAsync();
        }

        protected override SortDefinition<Series> BuildDefaultSort()
        {
            var sort = PropertySelector<Series>.Start(z => z)
                .SelectMany(z => z.Videos)
                .Select(z => z.Created)
                .ToString();
            Debug.Assert(sort == "Videos.Created");
            return Builders<Series>.Sort.Descending(sort);
        }

        public async Task<IEnumerable<Series>> ListTrackingAsync()
        {
            var builder = Builders<Series>.Filter;

            var filter = builder.Eq("Videos.IsTracking", true);

            return await (await this.Collection.FindAsync(
                filter,
                options: new FindOptions<Series, Series>()
                {
                    Sort = this.BuildDefaultSort()
                }))
                .ToListAsync();
        }
    }
}