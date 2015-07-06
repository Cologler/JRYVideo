using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class FlagManager : JryObjectManager<JryFlag, IFlagDataSourceProvider>
    {
        public FlagManager(IFlagDataSourceProvider source)
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

        public async void SeriesManager_VideoInfoUpdated(object sender, IEnumerable<ChangeEventArgs<JryVideoInfo>> e)
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
                    JryFlagType.EntityFilmSource,
                    infos.Where(z => z.FilmSource != null).Select(z => z.FilmSource).ToList()
                },
                {
                    JryFlagType.EntityAudioSource,
                    infos.Where(z => z.AudioSource != null).Select(z => z.AudioSource).ToList()
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

        public async void VideoManager_EntitiesUpdated(object sender, IEnumerable<ChangeEventArgs<JryEntity>> e)
        {
            var dict = CalcFlagDictionary(BuildFlagDictionary(e.Select(z => z.New)), BuildFlagDictionary(e.Select(z => z.Old)));

            await this.ApplyFlagDictionaryAsync(dict);
        }

        public async void VideoManager_EntitiesRemoved(object sender, IEnumerable<JryEntity> e)
        {
            var dict = CalcFlagDictionary(null, BuildFlagDictionary(e));

            await this.ApplyFlagDictionaryAsync(dict);
        }
    }
}