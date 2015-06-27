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
            var dict = BuildFlagDictionary(BuildFlagDictionary(e));

            await this.ApplyFlagDictionaryAsync(dict);
        }

        public async void SeriesManager_VideoInfoRemoved(object sender, IEnumerable<JryVideoInfo> e)
        {
            var dict = BuildFlagDictionary(BuildFlagDictionary(e));

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

        private static Dictionary<JryFlagType, Dictionary<string, int>> BuildFlagDictionary(Dictionary<JryFlagType, List<string>> source)
        {
            var dic = ((JryFlagType[])Enum.GetValues(typeof(JryFlagType))).ToDictionary(z => z, z => new Dictionary<string, int>());

            foreach (var selector in source)
            {
                foreach (var value in selector.Value)
                {
                    if (dic[selector.Key].ContainsKey(value))
                        dic[selector.Key][value]++;
                    else
                    {
                        dic[selector.Key].Add(value, 1);
                    }
                }
            }

            return dic;
        }
    }
}