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
    public class MongoSeriesDataSource : MongoJryEntitySet<Series, Series.QueryParameter>, ISeriesSet
    {
        public MongoSeriesDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<Series> collection)
            : base(engine, collection)
        {
        }

        public override async Task<IEnumerable<Series>> QueryAsync(Series.QueryParameter parameter, int skip, int take)
        {
            switch (parameter.Mode)
            {
                case Series.QueryMode.EntityId:
                    var q3 = PropertySelector<Model.JryVideo>.Start()
                        .SelectMany(z => z.Entities)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q3 == "Entities.Id");
                    var it = await this.Engine.VideoCollection.FindAsync(Builders<Model.JryVideo>.Filter.Eq(q3, parameter.Keyword));
                    var en = (await it.ToListAsync()).FirstOrDefault();
                    if (en == null) return Enumerable.Empty<Series>();
                    parameter = new Series.QueryParameter(parameter.OriginText, Series.QueryMode.VideoId, en.Id);
                    break;
            }

            return await base.QueryAsync(parameter, skip, take);
        }

        protected override IEnumerable<FilterDefinition<Series>> BuildFilters(Series.QueryParameter parameter)
        {
            var builder = Builders<Series>.Filter;

            switch (parameter.Mode)
            {
                case Series.QueryMode.OriginText:
                    var q1 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Names)
                        .ToString();
                    Debug.Assert(q1 == "Videos.Names");
                    yield return builder.Regex(z => z.Names,
                        new BsonRegularExpression(new Regex(Regex.Escape(parameter.Keyword), RegexOptions.IgnoreCase)));
                    yield return builder.Regex(q1,
                        new BsonRegularExpression(new Regex(Regex.Escape(parameter.Keyword), RegexOptions.IgnoreCase)));
                    yield break;

                case Series.QueryMode.SeriesId:
                    yield return builder.Eq(z => z.Id, parameter.Keyword);
                    yield break;

                case Series.QueryMode.VideoId:
                    var q2 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Id)
                        .ToString();
                    Debug.Assert(q2 == "Videos.Id");
                    yield return builder.Eq(q2, parameter.Keyword);
                    yield break;

                case Series.QueryMode.DoubanId:
                    var q5 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.DoubanId)
                        .ToString();
                    Debug.Assert(q5 == "Videos.DoubanId");
                    yield return builder.Eq(q5, parameter.Keyword);
                    yield break;

                case Series.QueryMode.Tag:
                    var q6 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Tags)
                        .ToString();
                    Debug.Assert(q6 == "Videos.Tags");
                    yield return builder.Eq(nameof(Series.Tags), parameter.Keyword);
                    yield return builder.Eq(q6, parameter.Keyword);
                    yield break;

                case Series.QueryMode.Any:
                    throw new InvalidOperationException();

                case Series.QueryMode.VideoType:
                    var q7 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Type)
                        .ToString();
                    Debug.Assert(q7 == "Videos.Type");
                    yield return builder.Eq(q7, parameter.Keyword);
                    yield break;

                case Series.QueryMode.VideoYear:
                    var year = Series.QueryParameter.GetYear(parameter.Keyword);
                    var q8 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Year)
                        .ToString();
                    Debug.Assert(q8 == "Videos.Year");
                    yield return builder.Eq(q8, year);
                    yield break;

                case Series.QueryMode.ImdbId:
                    var q9 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.ImdbId)
                        .ToString();
                    Debug.Assert(q9 == "Videos.ImdbId");
                    yield return builder.Eq(q9, parameter.Keyword);
                    yield break;

                case Series.QueryMode.Star:
                    var q10 = PropertySelector<Series>.Start()
                        .SelectMany(z => z.Videos)
                        .Select(z => z.Star)
                        .ToString();
                    Debug.Assert(q10 == "Videos.Star");
                    var stars = Series.QueryParameter.GetStar(parameter.Keyword);
                    Debug.Assert(stars.Length > 0);
                    yield return builder.In(q10, stars);
                    yield break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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