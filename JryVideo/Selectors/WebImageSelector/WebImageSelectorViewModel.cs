using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Core;
using JryVideo.Core.TheTVDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Selectors.WebImageSelector
{
    public class WebImageSelectorViewModel : JasilyViewModel
    {
        public ObservableCollection<string> Urls { get; }
            = new ObservableCollection<string>();

        public string SelectedUrl { get; set; }

        public void Load(IEnumerable<string> urls)
        {
            this.Urls.Reset(urls);
        }

        public async void BeginLoadPosterByImdbId(TheTVDBClient client, string imdbId)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (imdbId == null) throw new ArgumentNullException(nameof(imdbId));

            var videos = (await client.GetSeriesByImdbIdAsync(imdbId)).ToArray();
            if (videos.Length == 0) return;

            var urls = (await videos[0].GetBannersAsync(JryVideoCore.Current.TheTVDBClient))
                .Where(z => z.BannerType == BannerType.Poster)
                .Select(z => z.BuildUrl(client))
                .ToArray();
            this.Load(urls);
        }

        public async void BeginLoadFanartByImdbId(TheTVDBClient client, string index, params string[] imdbIds)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            foreach (var imdbId in imdbIds)
            {
                if (await this.LoadFanartByImdbIdAsync(client, imdbId, index)) return;
            }

            foreach (var imdbId in imdbIds)
            {
                if (await this.LoadFanartByImdbIdAsync(client, imdbId)) return;
            }
        }

        private async Task<bool> LoadFanartByImdbIdAsync(TheTVDBClient client, string imdbId, string index = null)
        {
            var videos = (await client.GetSeriesByImdbIdAsync(imdbId)).ToArray();
            if (videos.Length != 0)
            {
                var urls = (await videos[0].GetBannersAsync(JryVideoCore.Current.TheTVDBClient))
                    .Where(z => z.BannerType == BannerType.Fanart)
                    .Where(z => index == null || z.Season == index)
                    .Select(z => z.BuildUrl(client))
                    .ToArray();
                if (urls.Length > 0)
                {
                    this.Load(urls);
                    return true;
                }
            }
            return false;
        }
    }
}