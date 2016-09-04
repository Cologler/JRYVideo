using System;
using System.Collections.Generic;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;

namespace JryVideo.Data.MongoDb
{
    public sealed class MongoResourceDataSource : MongoJryEntitySet<Resource, Resource.QueryParameter>, IResourceDataSource
    {
        public MongoResourceDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<Resource> collection)
            : base(engine, collection)
        {
        }

        protected override IEnumerable<FilterDefinition<Resource>> BuildFilters(Resource.QueryParameter parameter)
        {
            if (parameter.VideoId != null)
            {
                yield return this.FilterDefinitionBuilder.AnyEq(z => z.VideoIds, parameter.VideoId);
            }

            if (parameter.FlagValue != null)
            {
                switch (parameter.FlagType)
                {
                    case JryFlagType.SeriesTag:
                    case JryFlagType.VideoYear:
                    case JryFlagType.VideoType:
                    case JryFlagType.VideoTag:
                        throw new NotSupportedException();

                    case JryFlagType.ResourceResolution:
                        yield return this.FilterDefinitionBuilder.Eq(z => z.Resolution, parameter.FlagValue);
                        break;

                    case JryFlagType.ResourceQuality:
                        yield return this.FilterDefinitionBuilder.Eq(z => z.Quality, parameter.FlagValue);
                        break;

                    case JryFlagType.ResourceExtension:
                        yield return this.FilterDefinitionBuilder.Eq(z => z.Extension, parameter.FlagValue);
                        break;

                    case JryFlagType.ResourceFansub:
                        yield return this.FilterDefinitionBuilder.AnyEq(z => z.Fansubs, parameter.FlagValue);
                        break;

                    case JryFlagType.ResourceSubTitleLanguage:
                        yield return this.FilterDefinitionBuilder.AnyEq(z => z.SubTitleLanguages, parameter.FlagValue);
                        break;

                    case JryFlagType.ResourceTrackLanguage:
                        yield return this.FilterDefinitionBuilder.AnyEq(z => z.TrackLanguages, parameter.FlagValue);
                        break;

                    case JryFlagType.ResourceAudioSource:
                        yield return this.FilterDefinitionBuilder.Eq(z => z.AudioSource, parameter.FlagValue);
                        break;

                    case JryFlagType.ResourceTag:
                        yield return this.FilterDefinitionBuilder.AnyEq(z => z.Tags, parameter.FlagValue);
                        break;


                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}