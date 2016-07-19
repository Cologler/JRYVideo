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
        private static readonly string VideosCreatedProperty = PropertySelector<Series>.Start(z => z)
            .SelectMany(z => z.Videos).Select(z => z.Created).ToString();

        private static readonly string VideosNamesProperty = PropertySelector<Series>.Start()
            .SelectMany(z => z.Videos).Select(z => z.Names).ToString();

        private static readonly string VideosIdProperty = PropertySelector<Series>.Start()
            .SelectMany(z => z.Videos).Select(z => z.Id).ToString();

        private static readonly string VideosDoubanIdProperty = PropertySelector<Series>.Start()
            .SelectMany(z => z.Videos).Select(z => z.DoubanId).ToString();

        private static readonly string VideosTagsProperty = PropertySelector<Series>.Start()
            .SelectMany(z => z.Videos).Select(z => z.Tags).ToString();

        private static readonly string VideosTypeProperty = PropertySelector<Series>.Start()
            .SelectMany(z => z.Videos).Select(z => z.Type).ToString();

        private static readonly string VideosYearProperty = PropertySelector<Series>.Start()
            .SelectMany(z => z.Videos).Select(z => z.Year).ToString();

        private static readonly string VideosImdbIdProperty = PropertySelector<Series>.Start()
            .SelectMany(z => z.Videos).Select(z => z.ImdbId).ToString();

        private static readonly string VideosStarProperty = PropertySelector<Series>.Start()
            .SelectMany(z => z.Videos).Select(z => z.Star).ToString();

        private static readonly string EntitiesIdProperty = PropertySelector<Model.JryVideo>.Start()
            .SelectMany(z => z.Entities).Select(z => z.Id).ToString();

        public MongoSeriesDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<Series> collection)
            : base(engine, collection)
        {
            Debug.Assert(VideosCreatedProperty == "Videos.Created");
            Debug.Assert(VideosNamesProperty == "Videos.Names");
            Debug.Assert(VideosIdProperty == "Videos.Id");
            Debug.Assert(VideosDoubanIdProperty == "Videos.DoubanId");
            Debug.Assert(VideosTagsProperty == "Videos.Tags");
            Debug.Assert(VideosTypeProperty == "Videos.Type");
            Debug.Assert(VideosYearProperty == "Videos.Year");
            Debug.Assert(VideosImdbIdProperty == "Videos.ImdbId");
            Debug.Assert(VideosStarProperty == "Videos.Star");
            Debug.Assert(EntitiesIdProperty == "Entities.Id");
        }

        public override async Task<IEnumerable<Series>> QueryAsync(Series.QueryParameter parameter, int skip, int take)
        {
            switch (parameter.Mode)
            {
                case Series.QueryMode.EntityId:
                    var it = await this.Engine.VideoCollection.FindAsync(Builders<Model.JryVideo>.Filter.Eq(
                        EntitiesIdProperty, parameter.Keyword));
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
                    yield return builder.Regex(z => z.Names,
                        new BsonRegularExpression(new Regex(Regex.Escape(parameter.Keyword), RegexOptions.IgnoreCase)));
                    yield return builder.Regex(VideosNamesProperty,
                        new BsonRegularExpression(new Regex(Regex.Escape(parameter.Keyword), RegexOptions.IgnoreCase)));
                    yield break;

                case Series.QueryMode.SeriesId:
                    yield return builder.Eq(z => z.Id, parameter.Keyword);
                    yield break;

                case Series.QueryMode.VideoId:
                    yield return builder.Eq(VideosIdProperty, parameter.Keyword);
                    yield break;

                case Series.QueryMode.DoubanId:
                    yield return builder.Eq(VideosDoubanIdProperty, parameter.Keyword);
                    yield break;

                case Series.QueryMode.Tag:
                    yield return builder.Eq(nameof(Series.Tags), parameter.Keyword);
                    yield return builder.Eq(VideosTagsProperty, parameter.Keyword);
                    yield break;

                case Series.QueryMode.Any:
                    throw new InvalidOperationException();

                case Series.QueryMode.VideoType:
                    yield return builder.Eq(VideosTypeProperty, parameter.Keyword);
                    yield break;

                case Series.QueryMode.VideoYear:
                    var year = Series.QueryParameter.GetYear(parameter.Keyword);
                    yield return builder.Eq(VideosYearProperty, year);
                    yield break;

                case Series.QueryMode.ImdbId:
                    yield return builder.Eq(VideosImdbIdProperty, parameter.Keyword);
                    yield break;

                case Series.QueryMode.Star:
                    var stars = Series.QueryParameter.GetStar(parameter.Keyword);
                    Debug.Assert(stars.Length > 0);
                    yield return builder.In(VideosStarProperty, stars);
                    yield break;

                case Series.QueryMode.WorldLineId:
                    yield return builder.Eq(z => z.WorldLineId, parameter.Keyword);
                    yield break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override SortDefinition<Series> BuildDefaultSort() => Builders<Series>.Sort.Descending(VideosCreatedProperty);

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