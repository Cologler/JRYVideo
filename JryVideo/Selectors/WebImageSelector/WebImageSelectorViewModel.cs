using Jasily.ComponentModel;
using JryVideo.Core;
using JryVideo.Core.TheTVDB;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Selectors.WebImageSelector
{
    public class WebImageSelectorViewModel : JasilyViewModel
    {
        public WebImageSelectorViewModel()
        {
            this.SetHeader("loading...");
        }

        private string header = "select a image";

        public string Header
        {
            get { return this.header; }
            private set { this.SetPropertyRef(ref this.header, value); }
        }

        public void SetHeader(string msg) => this.Header = $"select a image ({msg})";

        public ObservableCollection<string> Urls { get; }
            = new ObservableCollection<string>();

        public string SelectedUrl { get; set; }

        public void Load(IEnumerable<string> urls)
        {
            this.Urls.Reset(urls);
            this.SetHeader(this.Urls.Count.ToString());
        }

        public async void BeginLoadPosterByImdbId(TheTVDBClient client, string imdbId)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (imdbId == null) throw new ArgumentNullException(nameof(imdbId));

            var videos = (await client.GetSeriesByImdbIdAsync(imdbId)).ToArray();
            if (videos.Length == 0)
            {
                this.Load(Enumerable.Empty<string>());
            }
            else
            {
                var urls = (await videos[0].GetBannersAsync(JryVideoCore.Current.GetTheTVDBClient()))
                    .Where(z => z.BannerType == BannerType.Poster)
                    .Select(z => z.BuildUrl(client))
                    .ToArray();
                this.Load(urls);
            }
        }

        public async void BeginLoadFanartByImdbId(TheTVDBClient client, string index, params RemoteId[] ids)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            foreach (var id in ids)
            {
                if (await this.LoadFanartByImdbIdAsync(client, id, index)) return;
            }

            foreach (var id in ids)
            {
                if (await this.LoadFanartByImdbIdAsync(client, id)) return;
            }

            this.Load(Enumerable.Empty<string>());
        }

        private async Task<bool> LoadFanartByImdbIdAsync(TheTVDBClient client, RemoteId id, string index = null)
        {
            var seriesId = await client.TryGetTheTVDBSeriesIdByRemoteIdAsync(id);
            if (seriesId == null) return false;

            var urls = (await client.GetBannersBySeriesIdAsync(seriesId)).Where(z => z.BannerType == BannerType.Fanart)
                .Where(z => index == null || z.Season == index)
                .Select(z => z.BuildUrl(client))
                .ToArray();
            if (urls.Length > 0)
            {
                this.Load(urls);
                return true;
            }
            return false;
        }
    }
}