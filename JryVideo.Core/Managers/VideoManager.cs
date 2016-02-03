using JryVideo.Data.DataSources;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.EventArgses;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public sealed class VideoManager : AutoInsertVideoInfoAttachedManager<Model.JryVideo, IFlagableSet<Model.JryVideo>>
    {
        public event EventHandler<IEnumerable<JryEntity>> EntitiesCreated;
        public event EventHandler<IEnumerable<ChangingEventArgs<JryEntity>>> EntitiesUpdated;
        public event EventHandler<IEnumerable<JryEntity>> EntitiesRemoved;

        public VideoManager(IFlagableSet<Model.JryVideo> source)
            : base(source)
        {
        }

        public async void SeriesManager_VideoInfoRemoved(object sender, IEnumerable<JryVideoInfo> e)
        {
            foreach (var info in e)
            {
                await this.RemoveAsync(info.Id);
            }
        }

        public override async Task<bool> InsertAsync(Model.JryVideo obj)
        {
            if (await base.InsertAsync(obj))
            {
                if (obj.Entities.Count > 0)
                    this.EntitiesCreated.BeginFire(this, obj.Entities.ToArray());

                return true;
            }

            return false;
        }

        protected override async Task<bool> InsertAsync(IEnumerable<Model.JryVideo> objs)
        {
            var videos = objs as Model.JryVideo[] ?? objs.ToArray();
            if (await base.InsertAsync(videos))
            {
                var entites = videos.SelectMany(z => z.Entities).ToArray();

                if (entites.Length > 0)
                    this.EntitiesCreated.BeginFire(this, entites);

                return true;
            }

            return false;
        }

        public override async Task<bool> UpdateAsync(Model.JryVideo obj)
        {
            var old = await this.FindAsync(obj.Id);

            if (await base.UpdateAsync(obj))
            {
                var oldEntities = old.Entities.ToDictionary(z => z.Id);
                var newEntities = obj.Entities.ToDictionary(z => z.Id);

                var oldEntitiesIds = oldEntities.Keys.ToArray();
                var newEntitiesIds = newEntities.Keys.ToArray();

                var onlyOldIds = oldEntitiesIds.Except(newEntitiesIds).ToArray(); // 只在 old
                var onlyNewIds = newEntitiesIds.Except(oldEntitiesIds).ToArray(); // 只在 new
                var bothIds = oldEntitiesIds.Intersect(newEntitiesIds).ToArray(); // 交集

                if (onlyOldIds.Length > 0)
                {
                    var items = onlyOldIds.Select(id => oldEntities[id]).ToArray();
                    this.EntitiesRemoved.BeginFire(this, items);
                }

                if (onlyNewIds.Length > 0)
                {
                    var items = onlyNewIds.Select(id => newEntities[id]).ToArray();
                    this.EntitiesCreated.BeginFire(this, items);
                }

                if (bothIds.Length > 0)
                {
                    var items = bothIds.Select(id => new ChangingEventArgs<JryEntity>(oldEntities[id], newEntities[id])).ToArray();
                    this.EntitiesUpdated.BeginFire(this, items);
                }

                return true;
            }

            return false;
        }

        public EntityManager GetEntityManager(Model.JryVideo obj)
        {
            return new EntityManager(new EntityJryEntitySetSet(this, obj));
        }

        internal class EntityJryEntitySetSet : SubObjectSetProvider<Model.JryVideo, JryEntity>
        {
            public EntityJryEntitySetSet(VideoManager videoManager, Model.JryVideo video)
                : base(videoManager, video)
            {
            }

            public bool IsExists(JryEntity entity)
            {
                return this.Parent.Entities.Any(z => Same(z, entity));
            }

            private static bool Same(JryEntity left, JryEntity right)
            {
                return ReferenceEquals(left, right) ||
                       left.AudioSource == right.AudioSource && left.Extension == right.Extension &&
                       left.Quality == right.Quality && left.Resolution == right.Resolution &&
                       (left.Fansubs.SequenceEqual(right.Fansubs) &&
                        left.SubTitleLanguages.SequenceEqual(right.SubTitleLanguages) &&
                        left.TrackLanguages.SequenceEqual(right.TrackLanguages) &&
                        left.Format.NormalEquals(right.Format));
            }
        }

        public async void FlagManager_FlagChanged(object sender, EventArgs<JryFlagType, string, string> e)
        {
            var type = e.Value1;
            switch (type)
            {
                // can not change
                case JryFlagType.EntityResolution:
                case JryFlagType.EntityExtension:
                case JryFlagType.EntityAudioSource:
                case JryFlagType.EntityQuality:
                case JryFlagType.VideoYear:
                    throw new NotSupportedException();

                case JryFlagType.VideoType:
                    return;

                case JryFlagType.EntityFansub:
                case JryFlagType.EntitySubTitleLanguage:
                case JryFlagType.EntityTrackLanguage:
                case JryFlagType.EntityTag:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var oldValue = e.Value2;
            var newValue = e.Value3;
            var videos = await this.Source.QueryAsync(type, e.Value2);
            Action<List<string>> changeValue = (z) =>
            {
                var a = z.ToArray();
                for (var i = 0; i < a.Length; i++)
                {
                    if (a[i] == oldValue)
                    {
                        z[i] = newValue;
                        return;
                    }
                }
            };
            Action<Model.JryEntity> changing;
            switch (type)
            {
                case JryFlagType.EntityFansub:
                    changing = z => changeValue(z.Fansubs);
                    break;

                case JryFlagType.EntitySubTitleLanguage:
                    changing = z => changeValue(z.SubTitleLanguages);
                    break;

                case JryFlagType.EntityTrackLanguage:
                    changing = z => changeValue(z.TrackLanguages);
                    break;

                case JryFlagType.EntityTag:
                    changing = z => changeValue(z.Tags);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var video in videos)
            {
                video.Entities?.ForEach(changing);
                await this.UpdateAsync(video);
            }
        }
    }
}