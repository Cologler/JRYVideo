using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class SeriesManager : JryObjectManager<JrySeries, IDataSourceProvider<JrySeries>>
    {
        public DataCenter DataCenter { get; private set; }

        public SeriesManager(DataCenter dataCenter, IDataSourceProvider<JrySeries> source)
            : base(source)
        {
            this.DataCenter = dataCenter;
        }

        public async override Task<bool> InsertAsync(JrySeries series)
        {
            if (await base.InsertAsync(series))
            {
                var dic = BuildCounterDictionary(series);

                await this.RefMathByCounterDictionary(dic);

                return true;
            }

            return false;
        }

        public static void BuildSeriesMetaData(JrySeries series)
        {
            SeriesAction(series, BuildObjectMetaData);
        }

        private static void BuildObjectMetaData(JryObject obj)
        {
            if (obj != null && !obj.IsMetaDataBuilded())
            {
                obj.BuildMetaData();
            }
        }

        private static void SeriesAction(JrySeries series, Action<JryObject> action)
        {
            if (series == null) return;

            var func = new Func<JryObject, bool>(z =>
            {
                action(z);
                return true;
            });

            var j = SeriesFunc(series, func).ToArray();
        }
        private static IEnumerable<T> SeriesFunc<T>(JrySeries series, Func<JryObject, T> func)
        {
            if (series == null) return Enumerable.Empty<T>();

            if (series.Videos == null)
                return new [] { func(series) };

            return new[] { func(series) }
                .Concat(series.Videos.Select(z => func(z)))
                .Concat(series.Videos.Where(z => z.Entities != null).SelectMany(x => x.Entities).Select(c => func(c)))
                .ToArray();
        }
        private static IEnumerable<T> SeriesFunc<T>(JrySeries series, Func<JryObject, IEnumerable<T>> func)
        {
            if (series == null) return Enumerable.Empty<T>();

            if (series.Videos == null)
                return func(series).ToArray();

            return func(series)
                .Concat(series.Videos.SelectMany(z => func(z)))
                .Concat(series.Videos.Where(z => z.Entities != null).SelectMany(x => x.Entities).SelectMany(c => func(c)))
                .ToArray();
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

        public async override Task<bool> UpdateAsync(JrySeries series)
        {
            var old = await this.QueryAsync(series.Id);

            if (await base.UpdateAsync(series))
            {
                var dic = BuildCounterDictionary(series, old);

                await this.RefMathByCounterDictionary(dic);

                return true;
            }

            return false;
        }

        public async Task<bool> MergeAsync(JrySeries dest, JrySeries source)
        {
            return await Task.Run(async () =>
            {
                dest.Names.AddRange(source.Names);
                dest.Names = dest.Names.Distinct().ToList();

                dest.Videos.AddRange(source.Videos);

                return await this.UpdateAsync(dest);
            });
        }
    }
}