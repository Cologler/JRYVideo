using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Core;
using JryVideo.Core.TheTVDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JryVideo.Selectors.WebImageSelector
{
    public class WebImageSelectorViewModel : JasilyViewModel
    {
        public ObservableCollection<string> Urls { get; }
            = new ObservableCollection<string>();

        public JasilyCollectionView<byte[]> Buffers { get; }
            = new JasilyCollectionView<byte[]>();

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

            var x = (await videos[0].GetBannersAsync(JryVideoCore.Current.TheTVDBClient))?
                .Where(z => z.BannerType == BannerType.Poster)
                .Select(z => z.BuildUrl(client))
                .ToArray();
            if (x != null) this.Urls.Reset(x);
        }
    }
}