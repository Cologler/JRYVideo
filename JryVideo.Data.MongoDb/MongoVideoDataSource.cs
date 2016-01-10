using JryVideo.Data.DataSources;
using JryVideo.Model;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JryVideo.Data.MongoDb
{
    public class MongoVideoDataSource : MongoJryEntitySet<Model.JryVideo>, IFlagableSet<Model.JryVideo>
    {
        public MongoVideoDataSource(JryVideoMongoDbDataEngine engine, IMongoCollection<Model.JryVideo> collection)
            : base(engine, collection)
        {
        }

        public async Task<IEnumerable<Model.JryVideo>> QueryAsync(JryFlagType type, string flag)
        {
            FilterDefinition<Model.JryVideo> filter;

            switch (type)
            {
                case JryFlagType.VideoYear:
                case JryFlagType.VideoType:
                    throw new NotSupportedException();

                case JryFlagType.EntityResolution:
                    filter = Builders<Model.JryVideo>.Filter.Eq(nameof(Model.JryVideo.Entities) + "." + nameof(JryEntity.Resolution), flag);
                    break;

                case JryFlagType.EntityFilmSource:
                    filter = Builders<Model.JryVideo>.Filter.Eq(nameof(Model.JryVideo.Entities) + "." + nameof(JryEntity.FilmSource), flag);
                    break;
                case JryFlagType.EntityExtension:
                    filter = Builders<Model.JryVideo>.Filter.Eq(nameof(Model.JryVideo.Entities) + "." + nameof(JryEntity.Extension), flag);
                    break;
                case JryFlagType.EntityFansub:
                    filter = Builders<Model.JryVideo>.Filter.Eq(nameof(Model.JryVideo.Entities) + "." + nameof(JryEntity.Fansubs), flag);
                    break;

                case JryFlagType.EntitySubTitleLanguage:
                    filter = Builders<Model.JryVideo>.Filter.Eq(nameof(Model.JryVideo.Entities) + "." + nameof(JryEntity.SubTitleLanguages), flag);
                    break;

                case JryFlagType.EntityTrackLanguage:
                    filter = Builders<Model.JryVideo>.Filter.Eq(nameof(Model.JryVideo.Entities) + "." + nameof(JryEntity.TrackLanguages), flag);
                    break;

                case JryFlagType.EntityAudioSource:
                    filter = Builders<Model.JryVideo>.Filter.Eq(nameof(Model.JryVideo.Entities) + "." + nameof(JryEntity.AudioSource), flag);
                    break;

                case JryFlagType.EntityTag:
                    filter = Builders<Model.JryVideo>.Filter.Eq(nameof(Model.JryVideo.Entities) + "." + nameof(JryEntity.Tags), flag);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return await (await this.Collection.FindAsync(filter)).ToListAsync();
        }
    }
}