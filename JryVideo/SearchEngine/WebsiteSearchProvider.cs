using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace JryVideo.SearchEngine
{
    public sealed class WebsiteSearchProvider : ISearchEngineProvider
    {
        public IEnumerable<ISearchEngine> Create()
        {
            var configs = ((App)Application.Current).UserConfig?.SearchEngines?.ToArray();
            if (configs == null || configs.Length == 0) yield break;
            foreach (var config in configs)
            {
                var url = config.Url;
                if (url.IsNullOrWhiteSpace()) continue;
                var name = config.Name;
                if (config.Name.IsNullOrWhiteSpace())
                {
                    var domain = url.Split("://", 2).Last().Split("/", 2).First().Split(".");
                    name = domain.Length > 1 ? domain[domain.Length - 2] : domain[0];

                }
                yield return new WebsiteSearch(name, url, 0);
            }
        }

        public int Order => 2;
    }
}