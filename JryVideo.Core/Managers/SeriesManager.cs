using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class SeriesManager
    {
        public DataCenter DataCenter { get; private set; }

        public IDataSourceProvider<JrySeries> Source { get; private set; }

        public SeriesManager(DataCenter dataCenter, IDataSourceProvider<JrySeries> source)
        {
            this.DataCenter = dataCenter;
            this.Source = source;
        }

        public async Task<bool> InsertAsync(JrySeries series)
        {
            if (await this.Source.InsertAsync(series))
            {
                var dic = BuildCounterDictionary(series);

                await this.RefMathByCounterDictionary(dic);

                return true;
            }

            return false;
        }

        private async Task RefMathByCounterDictionary(Dictionary<JryCounterType, Dictionary<string, int>> dic)
        {
            var counterDataSource = this.DataCenter.ProviderManager.GetCounterDataSourceProvider();

            foreach (var item in dic)
            {
                foreach (var value in item.Value)
                {
                    await counterDataSource.RefMathAsync(item.Key, value.Key, value.Value);
                }
            }
        }

        private static Dictionary<JryCounterType, List<string>> BuildCounterValueListDictionary(JrySeries series)
        {
            return new Dictionary<JryCounterType, List<string>>()
            {
                // single in video
                {
                    JryCounterType.VideoType,
                    series.Videos
                        .Select(z => z.Type.ToString())
                        .ToList()
                },
                {
                    JryCounterType.VideoYear,
                    series.Videos
                        .Select(z => z.Year.ToString())
                        .ToList()
                },

                // single in entity
                {
                    JryCounterType.EntityExtension,
                    series.Videos
                        .SelectMany(z => z.Entities)
                        .Where(z => !String.IsNullOrWhiteSpace(z.Extension))
                        .Select(z => z.Extension)
                        .ToList()
                },
                {
                    JryCounterType.EntityFilmSource,
                    series.Videos
                        .SelectMany(z => z.Entities)
                        .Where(z => !String.IsNullOrWhiteSpace(z.FilmSource))
                        .Select(z => z.FilmSource)
                        .ToList()
                },
                {
                    JryCounterType.EntityResolution,
                    series.Videos
                        .SelectMany(z => z.Entities)
                        .Where(z => !String.IsNullOrWhiteSpace(z.Resolution))
                        .Select(z => z.Resolution)
                        .ToList()
                },

                // muilt in entity
                {
                    JryCounterType.EntityFansub,
                    series.Videos
                        .SelectMany(z => z.Entities)
                        .SelectMany(x => x.Fansubs)
                        .ToList()
                },
                {
                    JryCounterType.EntitySubTitleLanguage,
                    series.Videos
                        .SelectMany(z => z.Entities)
                        .SelectMany(x => x.SubTitleLanguages)
                        .ToList()
                },
                {
                    JryCounterType.EntityTrackLanguage,
                    series.Videos
                        .SelectMany(z => z.Entities)
                        .SelectMany(x => x.TrackLanguages)
                        .ToList()
                },
            };
        }

        private static Dictionary<JryCounterType, Dictionary<string, int>> BuildCounterDictionary(JrySeries series, JrySeries oldSeries = null)
        {
            var dic = ((JryCounterType[]) Enum.GetValues(typeof(JryCounterType))).ToDictionary(z => z, z => new Dictionary<string, int>());

            foreach (var selector in BuildCounterValueListDictionary(series))
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

            if (oldSeries != null)
            {
                foreach (var selector in BuildCounterValueListDictionary(oldSeries))
                {
                    foreach (var value in selector.Value)
                    {
                        if (dic[selector.Key].ContainsKey(value))
                            dic[selector.Key][value]--;
                        else
                        {
                            dic[selector.Key].Add(value, -1);
                        }
                    }
                }

                return dic;
            }

            return dic;
        }

        public async Task<IEnumerable<JrySeries>> LoadAsync()
        {
            return await this.Source.QueryAsync(0, Int32.MaxValue);
        }

        public async Task<bool> UpdateAsync(JrySeries series)
        {
            var old = await this.Source.QueryAsync(series.Id);

            if (await this.Source.UpdateAsync(series))
            {
                var dic = BuildCounterDictionary(series, old);

                await this.RefMathByCounterDictionary(dic);

                return true;
            }

            return false;
        }
    }
}