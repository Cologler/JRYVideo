using System;
using System.Collections.Generic;
using System.EventArgses;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public sealed class FlagManager : JryObjectManager<JryFlag, IFlagSet>
    {
        public FlagManager(IFlagSet source)
            : base(source)
        {
        }

        public void Initialize(DataCenter dataCenter)
        {
            dataCenter.SeriesManager.ItemCreated += this.SeriesManager_ItemCreated;
            dataCenter.SeriesManager.ItemUpdated += this.SeriesManager_ItemUpdated;
            dataCenter.SeriesManager.VideoInfoCreated += this.SeriesManager_VideoInfoCreated;
            dataCenter.SeriesManager.VideoInfoUpdated += this.SeriesManager_VideoInfoUpdated;
            dataCenter.SeriesManager.VideoInfoRemoved += this.SeriesManager_VideoInfoRemoved;
            dataCenter.VideoManager.EntitiesCreated += this.VideoManager_EntitiesCreated;
            dataCenter.VideoManager.EntitiesUpdated += this.VideoManager_EntitiesUpdated;
            dataCenter.VideoManager.EntitiesRemoved += this.VideoManager_EntitiesRemoved;
        }

        private async void SeriesManager_ItemUpdated(object sender, ChangingEventArgs<Series> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e.New), BuildFlagDictionary(e.Old));
            await this.ApplyFlagDictionaryAsync(dict);
        }

        private async void SeriesManager_ItemCreated(object sender, Series e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e));
            await this.ApplyFlagDictionaryAsync(dict);
        }

        public async Task<IEnumerable<JryFlag>> LoadAsync(JryFlagType type) => await this.Source.QueryAsync(type, 0, int.MaxValue);

        private async Task ApplyFlagDictionaryAsync(Dictionary<JryFlagType, Dictionary<string, int>> dict)
        {
            foreach (var item in dict)
            {
                foreach (var value in item.Value)
                {
                    await this.Source.IncrementAsync(item.Key, value.Key, value.Value);
                }
            }
        }

        private static Dictionary<JryFlagType, List<string>> BuildFlagDictionary(Series series)
        {
            if (series.Tags != null)
            {
                return new Dictionary<JryFlagType, List<string>>()
                {
                    {
                        JryFlagType.SeriesTag,
                        series.Tags.ToList()
                    }
                };
            }
            else
            {
                return new Dictionary<JryFlagType, List<string>>();
            }
        }

        private static Dictionary<JryFlagType, List<string>> BuildFlagDictionary(IEnumerable<JryVideoInfo> infos)
        {
            return new Dictionary<JryFlagType, List<string>>()
            {
                // single in video
                {
                    JryFlagType.VideoType,
                    infos.Select(z => z.Type).ToList()
                },
                {
                    JryFlagType.VideoYear,
                    infos.Select(z => z.Year.ToString()).ToList()
                },
                {
                    JryFlagType.VideoTag,
                    infos.Where(z => z.Tags != null).SelectMany(z => z.Tags).ToList()
                }
            };
        }

        private static Dictionary<JryFlagType, List<string>> BuildFlagDictionary(IEnumerable<JryEntity> infos)
        {
            return new Dictionary<JryFlagType, List<string>>()
            {
                // single in entity
                {
                    JryFlagType.EntityExtension,
                    infos.Select(z => z.Extension).ToList()
                },
                {
                    JryFlagType.EntityResolution,
                    infos.Select(z => z.Resolution).ToList()
                },
                {
                    JryFlagType.EntityQuality,
                    infos.Where(z => !string.IsNullOrEmpty(z.Quality)).Select(z => z.Quality).ToList()
                },
                {
                    JryFlagType.EntityAudioSource,
                    infos.Where(z => !string.IsNullOrEmpty(z.AudioSource)).Select(z => z.AudioSource).ToList()
                },

                // muilt in entity
                {
                    JryFlagType.EntityFansub,
                    infos.SelectMany(z => z.Fansubs).ToList()
                },
                {
                    JryFlagType.EntitySubTitleLanguage,
                    infos.SelectMany(z => z.SubTitleLanguages).ToList()
                },
                {
                    JryFlagType.EntityTrackLanguage,
                    infos.SelectMany(z => z.TrackLanguages).ToList()
                },
                {
                    JryFlagType.EntityTag,
                    infos.SelectMany(z => z.Tags).ToList()
                }
            };
        }

        private static Dictionary<JryFlagType, Dictionary<string, int>> CalcFlagDictionary(
            Dictionary<JryFlagType, List<string>> add = null, Dictionary<JryFlagType, List<string>> sub = null)
        {
            var dic = ((JryFlagType[])Enum.GetValues(typeof(JryFlagType))).ToDictionary(z => z, z => new Dictionary<string, int>());

            if (add != null)
            {
                foreach (var selector in add)
                {
                    foreach (var value in selector.Value)
                    {
                        dic[selector.Key].ValueMoveNext(value, 1, z => z + 1);
                    }
                }
            }

            if (sub != null)
            {
                foreach (var selector in sub)
                {
                    foreach (var value in selector.Value)
                    {
                        dic[selector.Key].ValueMoveNext(value, -1, z => z - 1);
                    }
                }
            }

            return dic;
        }

        private async void SeriesManager_VideoInfoCreated(object sender, IEnumerable<JryVideoInfo> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e));
            await this.ApplyFlagDictionaryAsync(dict);
        }

        private async void SeriesManager_VideoInfoUpdated(object sender, IEnumerable<ChangingEventArgs<JryVideoInfo>> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e.Select(z => z.New)), BuildFlagDictionary(e.Select(z => z.Old)));
            await this.ApplyFlagDictionaryAsync(dict);
        }

        private async void SeriesManager_VideoInfoRemoved(object sender, IEnumerable<JryVideoInfo> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e));
            await this.ApplyFlagDictionaryAsync(dict);
        }

        private async void VideoManager_EntitiesCreated(object sender, IEnumerable<JryEntity> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e));
            await this.ApplyFlagDictionaryAsync(dict);
        }

        private async void VideoManager_EntitiesUpdated(object sender, IEnumerable<ChangingEventArgs<JryEntity>> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e.Select(z => z.New)), BuildFlagDictionary(e.Select(z => z.Old)));
            await this.ApplyFlagDictionaryAsync(dict);
        }

        private async void VideoManager_EntitiesRemoved(object sender, IEnumerable<JryEntity> e)
        {
            var dict = CalcFlagDictionary(null, BuildFlagDictionary(e));
            await this.ApplyFlagDictionaryAsync(dict);
        }

        public static bool CanReplace(JryFlagType type)
        {
            switch (type)
            {
                case JryFlagType.VideoYear:

                case JryFlagType.EntityResolution:
                case JryFlagType.EntityExtension:
                case JryFlagType.EntityAudioSource:
                case JryFlagType.EntityQuality:
                    return false;

                case JryFlagType.SeriesTag:

                case JryFlagType.VideoType:
                case JryFlagType.VideoTag:
                    return false;

                case JryFlagType.EntityFansub:
                case JryFlagType.EntitySubTitleLanguage:
                case JryFlagType.EntityTrackLanguage:
                case JryFlagType.EntityTag:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public async Task<bool> ReplaceAsync(JryFlagType type, string oldName, string newName)
        {
            if (oldName == null) throw new ArgumentNullException(nameof(oldName));
            if (newName == null) throw new ArgumentNullException(nameof(newName));
            if (!CanReplace(type)) throw new NotSupportedException();
            if (oldName == newName) return true;

            // old
            var oldId = JryFlag.BuildFlagId(type, oldName);
            var oldFlag = await this.FindAsync(oldId);
            var count = oldFlag?.Count ?? 0;
            if (oldFlag != null)
            {
                await this.Source.RemoveAsync(oldId);
            }

            // new
            var newId = JryFlag.BuildFlagId(type, newName);
            var flag = await this.FindAsync(newId);
            bool ret;
            if (flag != null)
            {
                ret = await this.Source.IncrementAsync(type, newName, count);
            }
            else
            {
                flag = new JryFlag();
                flag.Type = type;
                flag.Value = newName;
                flag.Count = count;
                flag.BuildMetaData(true);
                ret = await this.InsertAsync(flag);
            }

            // exist
            if (ret)
            {
                this.FlagChanged?.Invoke(this, new EventArgs<JryFlagType, string, string>(type, oldName, newName));
            }
            return ret;
        }

        internal event EventHandler<EventArgs<JryFlagType, string, string>> FlagChanged;
    }
}