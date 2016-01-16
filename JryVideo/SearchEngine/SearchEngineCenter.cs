using JryVideo.Viewer.VideoViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JryVideo.SearchEngine
{
    public static class SearchEngineCenter
    {
        private static ISearchEngine[] AllEngines;

        static SearchEngineCenter()
        {
            App.UserConfigChanged += App_UserConfigChanged;
            Remap();
        }

        private static void App_UserConfigChanged(object sender, Configs.UserConfig e) => Remap();

        public static void Remap()
        {
            var engineType = typeof(ISearchEngine);
            var providerType = typeof(ISearchEngineProvider);

            AllEngines = typeof(VideoViewerPage).Assembly.DefinedTypes
                .Where(z => z.IsSealed)
                .Where(z => engineType.IsAssignableFrom(z) || providerType.IsAssignableFrom(z))
                .Where(z => z.GetTypeInfo().GetConstructor(new Type[0]) != null)
                .Select(z => (IOrder)Activator.CreateInstance(z))
                .OrderBy(z => z.Order)
                .SelectMany(GetEngines)
                .ToArray();
        }

        private static IEnumerable<ISearchEngine> GetEngines(IOrder item)
        {
            var p = item as ISearchEngineProvider;
            if (p != null)
            {
                foreach (var searchEngine in p.Create())
                {
                    yield return searchEngine;
                }
            }
            var e = item as ISearchEngine;
            if (e != null)
            {
                yield return e;
            }
        }

        public static IEnumerable<ISearchEngine> Engines => AllEngines;
    }
}