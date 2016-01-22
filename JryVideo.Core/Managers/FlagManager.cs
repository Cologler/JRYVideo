using JryVideo.Data.DataSources;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.EventArgses;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public class FlagManager : JryObjectManager<JryFlag, IFlagSet>
    {
        public FlagManager(IFlagSet source)
            : base(source)
        {
        }

        public async Task<IEnumerable<JryFlag>> LoadAsync(JryFlagType type)
        {
            return await this.Source.QueryAsync(type);
        }

        public async void SeriesManager_VideoInfoCreated(object sender, IEnumerable<JryVideoInfo> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e));

            await this.ApplyFlagDictionaryAsync(dict);
        }

        public async void SeriesManager_VideoInfoUpdated(object sender, IEnumerable<ChangingEventArgs<JryVideoInfo>> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e.Select(z => z.New)), BuildFlagDictionary(e.Select(z => z.Old)));

            await this.ApplyFlagDictionaryAsync(dict);
        }

        public async void SeriesManager_VideoInfoRemoved(object sender, IEnumerable<JryVideoInfo> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e));

            await this.ApplyFlagDictionaryAsync(dict);
        }

        private async Task ApplyFlagDictionaryAsync(Dictionary<JryFlagType, Dictionary<string, int>> dict)
        {
            foreach (var item in dict)
            {
                foreach (var value in item.Value)
                {
                    await this.Source.RefMathAsync(item.Key, value.Key, value.Value);
                }
            }
        }

        private static Dictionary<JryFlagType, List<string>> BuildFlagDictionary(IEnumerable<JryVideoInfo> infos)
        {
            return new Dictionary<JryFlagType, List<string>>()
            {
                // single in video
                {
                    JryFlagType.VideoType,
                    infos.Select(z => z.Type.ToString()).ToList()
                },
                {
                    JryFlagType.VideoYear,
                    infos.Select(z => z.Year.ToString()).ToList()
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
                        if (dic[selector.Key].ContainsKey(value))
                            dic[selector.Key][value]++;
                        else
                            dic[selector.Key].Add(value, 1);
                    }
                }
            }

            if (sub != null)
            {
                foreach (var selector in sub)
                {
                    foreach (var value in selector.Value)
                    {
                        if (dic[selector.Key].ContainsKey(value))
                            dic[selector.Key][value]--;
                        else
                            dic[selector.Key].Add(value, -1);
                    }
                }
            }

            return dic;
        }

        public async void VideoManager_EntitiesCreated(object sender, IEnumerable<JryEntity> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e));

            await this.ApplyFlagDictionaryAsync(dict);
        }

        public async void VideoManager_EntitiesUpdated(object sender, IEnumerable<ChangingEventArgs<JryEntity>> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e.Select(z => z.New)), BuildFlagDictionary(e.Select(z => z.Old)));

            await this.ApplyFlagDictionaryAsync(dict);
        }

        public async void VideoManager_EntitiesRemoved(object sender, IEnumerable<JryEntity> e)
        {
            var dict = CalcFlagDictionary(null, BuildFlagDictionary(e));

            await this.ApplyFlagDictionaryAsync(dict);
        }

        public async Task<bool> UpdateNameAsync(JryFlagType type, string oldName, string newName)
        {
            if (oldName == null) throw new ArgumentNullException(nameof(oldName));
            if (newName == null) throw new ArgumentNullException(nameof(newName));
            if (oldName == newName) return true;

            switch (type)
            {
                // can not change
                case JryFlagType.EntityResolution:
                case JryFlagType.EntityExtension:
                case JryFlagType.EntityAudioSource:
                case JryFlagType.EntityQuality:
                case JryFlagType.VideoYear:
                    return true;

                case JryFlagType.VideoType:
                case JryFlagType.EntityFansub:
                case JryFlagType.EntitySubTitleLanguage:
                case JryFlagType.EntityTrackLanguage:
                case JryFlagType.EntityTag:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            // old
            var oldId = JryFlag.BuildCounterId(type, oldName);
            var oldFlag = await this.FindAsync(oldId);
            var count = oldFlag?.Count ?? 0;
            if (oldFlag != null)
            {
                await this.Source.RemoveAsync(oldId);
            }

            // new
            var newId = JryFlag.BuildCounterId(type, newName);
            var flag = await this.FindAsync(newId);
            bool ret;
            if (flag != null)
            {
                ret = await this.Source.RefMathAsync(type, newName, count);
            }
            else
            {
                flag = new JryFlag();
                flag.Type = type;
                flag.Value = newName;
                flag.Count = count;
                flag.BuildMetaData(true);
                ret = await this.Source.InsertAsync(flag);
            }

            // exist
            if (ret)
            {
                this.FlagChanged.BeginFire(this, new EventArgs<JryFlagType, string, string>(type, oldName, newName));
            }
            return ret;
        }

        public event EventHandler<EventArgs<JryFlagType, string, string>> FlagChanged;
    }
}