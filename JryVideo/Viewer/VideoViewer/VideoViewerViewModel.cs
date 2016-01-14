using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.TheTVDB;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Viewer.VideoViewer
{
    public sealed class VideoViewerViewModel : JasilyViewModel
    {
        private VideoViewModel video;
        private BackgroundViewModel background;

        public VideoViewerViewModel(VideoInfoViewModel info)
        {
            this.InfoView = info;
            this.EntitesView = new JasilyCollectionView<ObservableCollectionGroup<string, EntityViewModel>>();
        }

        public VideoInfoViewModel InfoView { get; }

        public VideoViewModel Video
        {
            get { return this.video; }
            private set { this.SetPropertyRef(ref this.video, value); }
        }

        public BackgroundViewModel Background
        {
            get { return this.background; }
            set { this.SetPropertyRef(ref this.background, value); }
        }

        public async Task LoadAsync()
        {
            this.Background = new BackgroundViewModel(new BackgroundCover(this));

            await this.ReloadVideoAsync();
        }

        public async Task ReloadVideoAsync()
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.VideoManager;

            var video = await manager.FindAsync(this.InfoView.Source.Id);

            if (video == null)
            {
                this.Video = null;
            }
            else
            {
                this.Video = new VideoViewModel(video);

                this.EntitesView.Collection.Clear();
                this.EntitesView.Collection.AddRange(video.Entities
                    .Select(z => new EntityViewModel(z))
                    .GroupBy(v => v.Source.Resolution ?? "unknown")
                    .OrderBy(z => z.Key)
                    .Select(g => new ObservableCollectionGroup<string, EntityViewModel>(g.Key, g.OrderBy(this.CompareEntityViewModel))));
            }

            this.ReloadEpisodes();
        }

        public void ReloadEpisodes()
        {
            var video = this.Video?.Source;
            if (video == null)
            {
                this.Watcheds.Clear();
            }
            else
            {
                var watcheds = Enumerable.Range(1, this.InfoView.Source.EpisodesCount)
                    .Select(z => new WatchedEpisodeChecker(z))
                    .ToArray();
                if (video.Watcheds != null)
                {
                    foreach (var ep in video.Watcheds.Where(z => z <= this.InfoView.Source.EpisodesCount))
                    {
                        watcheds[ep - 1].IsWatched = true;
                    }
                }
                this.Watcheds.Reset(watcheds);
            }
        }

        public ObservableCollection<WatchedEpisodeChecker> Watcheds { get; }
            = new ObservableCollection<WatchedEpisodeChecker>();

        public JasilyCollectionView<ObservableCollectionGroup<string, EntityViewModel>> EntitesView { get; private set; }

        private int CompareEntityViewModel(EntityViewModel x, EntityViewModel y)
        {
            Debug.Assert(x != null, "x != null");
            Debug.Assert(y != null, "y != null");

            if (x.Source.Id == y.Source.Id) return 0;

            if (x.Source.Fansubs.Count > 0 && y.Source.Fansubs.Count > 0)
                return Comparer<string>.Default.Compare(x.Source.Fansubs[0], y.Source.Fansubs[0]);

            if (x.Source.Extension != y.Source.Extension)
                return Comparer<string>.Default.Compare(x.Source.Extension, y.Source.Extension);

            return -1;
        }

        public async void Flush()
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.VideoManager;

            var video = await manager.FindAsync(this.InfoView.Source.Id);

            if (video == null) return;

            var watched = this.Watcheds.Where(z => z.IsWatched)
                .Select(z => z.Episode)
                .OrderBy(z => z)
                .ToList();
            if (watched.Count == 0)
            {
                if (video.Watcheds != null)
                {
                    video.Watcheds = null;
                    await manager.UpdateAsync(video);
                    this.InfoView.RefreshProperties();
                }
            }
            else
            {
                if (video.Watcheds?.Count != watched.Count ||
                    watched.Where((t, i) => video.Watcheds[i] != t).Any())
                {
                    video.Watcheds = watched;
                    await manager.UpdateAsync(video);
                    this.InfoView.RefreshProperties();
                }
            }
        }

        public sealed class WatchedEpisodeChecker : NotifyPropertyChangedObject
        {
            public WatchedEpisodeChecker(int episode)
            {
                this.Episode = episode;
                this.EpisodeName = $"ep {episode}";
            }

            public int Episode { get; }

            public string EpisodeName { get; }

            public bool IsWatched { get; set; }

            public void SetIsWatchedAndNotify(bool value)
            {
                if (this.IsWatched == value) return;
                this.IsWatched = value;
                this.NotifyPropertyChanged(nameof(this.IsWatched));
            }
        }

        public sealed class BackgroundViewModel : HasCoverViewModel<BackgroundCover>
        {
            public BackgroundViewModel(BackgroundCover source)
                : base(source)
            {
            }

            protected override async Task<bool> TryAutoAddCoverAsync()
            {
                return await this.InternalTryAutoAddCoverAsync(this.Source.ImdbId) ||
                       await this.InternalTryAutoAddCoverAsync(this.Source.SeriesImdbId);
            }

            private async Task<bool> InternalTryAutoAddCoverAsync(string imdbId)
            {
                if (imdbId.IsNullOrWhiteSpace() || !imdbId.StartsWith("tt")) return false;

                var client = JryVideoCore.Current.TheTVDBClient;
                if (client == null) return false;

                var guid = await this.AutoGenerateCoverAsync(client, imdbId, this.Source.Index.ToString());
                if (guid != null)
                {
                    return await this.Source.UpdateImageIfEmptyAsync(guid);
                }
                return false;
            }

            private async Task<string> AutoGenerateCoverAsync(TheTVDBClient client, string imdbId, string index)
            {
                return await Task.Run(async () =>
                {
                    var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;
                    var x = (await coverManager.Source.QueryByDoubanIdAsync(JryCoverType.Background, imdbId))
                            .SingleOrDefault();
                    if (x != null) return x.Id;
                    foreach (var video in await client.GetSeriesByImdbIdAsync(imdbId))
                    {
                        var array = (await video.GetBannersAsync(client)).ToArray();
                        foreach (var banner in array.Where(z => z.Season == index)
                            .RandomSort()
                            .Concat(array.Where(z => z.Season != index).RandomSort()))
                        {
                            if (banner.BannerType == BannerType.Fanart)
                            {
                                var url = banner.BuildUrl(client);
                                var cover = new JryCover();
                                cover.CoverSourceType = JryCoverSourceType.Imdb;
                                cover.CoverType = JryCoverType.Background;
                                cover.DoubanId = this.Source.DoubanId;
                                cover.Uri = url;
                                cover.ImdbId = this.Source.ImdbId;

                                var guid = await coverManager.DownloadCoverAsync(cover);
                                if (guid != null) return guid;
                            }
                        }
                    }
                    return null;
                });
            }

            public override void BeginUpdateCover()
            {
                if (this.CoverValue != null) return;

                base.BeginUpdateCover();
            }
        }

        public sealed class BackgroundCover : IJryCoverParent
        {
            private readonly VideoViewerViewModel source;

            public BackgroundCover(VideoViewerViewModel source)
            {
                this.source = source;
            }

            public string CoverId
            {
                get { return this.source.InfoView.Source.BackgroundImageId; }
                set { this.source.InfoView.Source.BackgroundImageId = value; }
            }

            public string ImdbId => this.source.InfoView.Source.ImdbId;

            public string SeriesImdbId => this.source.InfoView.SeriesView.Source.ImdbId;

            public string DoubanId => this.source.InfoView.Source.DoubanId;

            public int Index => this.source.InfoView.Source.Index;

            public async Task<bool> UpdateImageIfEmptyAsync(string guid)
            {
                this.source.InfoView.Source.BackgroundImageId = guid;
                var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(
                    this.source.InfoView.SeriesView.Source);
                return await manager.UpdateAsync(this.source.InfoView.Source);
            }
        }
    }
}