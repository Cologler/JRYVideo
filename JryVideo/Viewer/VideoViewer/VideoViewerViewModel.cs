using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.Models;
using JryVideo.Core.TheTVDB;
using JryVideo.Model;
using JryVideo.Model.Interfaces;
using JryVideo.Selectors.WebImageSelector;

namespace JryVideo.Viewer.VideoViewer
{
    public sealed class VideoViewerViewModel : JasilyViewModel
    {
        private VideoViewModel video;

        public VideoViewerViewModel(VideoInfoViewModel info)
        {
            this.InfoView = info;
            this.EntitesView = new JasilyCollectionView<ObservableCollectionGroup<string, EntityViewModel>>();
            this.Background = new BackgroundViewModel(this);
            this.VideoRoleCollection = new VideoRoleCollectionViewModel(info.SeriesView.Source, info.Source)
            {
                VideoViewerViewModel = this
            };
        }

        public VideoInfoViewModel InfoView { get; }

        public VideoViewModel Video
        {
            get { return this.video; }
            private set { this.SetPropertyRef(ref this.video, value); }
        }

        public BackgroundViewModel Background { get; }

        public VideoRoleCollectionViewModel VideoRoleCollection { get; }

        public async Task LoadAsync()
        {
            await this.ReloadVideoAsync();
            await this.VideoRoleCollection.LoadAsync();
            await this.AutoCompleteAsync();
        }

        public async Task ReloadVideoAsync()
        {
            this.EntitesView.Collection.Clear();
            var video = await this.GetManagers().VideoManager.FindAsync(this.InfoView.Source.Id);
            if (video == null)
            {
                this.Video = null;
            }
            else
            {
                this.Video = new VideoViewModel(video);

                this.EntitesView.Collection.Reset(video.Entities
                    .Select(z => new EntityViewModel(z))
                    .GroupBy(v => v.Source.Resolution ?? "unknown")
                    .OrderBy(z => z.Key)
                    .Select(g => new ObservableCollectionGroup<string, EntityViewModel>(g.Key, g.OrderBy(this.CompareEntityViewModel))));
            }

            this.ReloadEpisodes();
        }

        public async Task AutoCompleteAsync()
        {
            await this.InfoView.AutoCompleteAsync();
            await this.VideoRoleCollection.AutoCompleteAsync();
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
                var episodesCount = this.InfoView.Source.EpisodesCount;
                var watcheds = Enumerable.Range(1, episodesCount)
                    .Select(z => new WatchedEpisodeChecker(z))
                    .ToArray();
                if (video.Watcheds != null)
                {
                    foreach (var ep in video.Watcheds.Where(z => z <= episodesCount))
                    {
                        watcheds[ep - 1].IsWatched = true;
                    }
                }
                var min = Math.Min(episodesCount, this.Watcheds.Count);
                if (min > 0)
                {
                    for (var i = 0; i < min; i++)
                    {
                        watcheds[i].IsWatched = this.Watcheds[i].IsWatched;
                    }
                }
                this.Watcheds.Reset(watcheds);
            }
        }

        public ObservableCollection<WatchedEpisodeChecker> Watcheds { get; }
            = new ObservableCollection<WatchedEpisodeChecker>();

        public void WatchAll() => this.Watcheds.ForEach(z => z.SetIsWatchedAndNotify(true));

        public void WatchReverse() => this.Watcheds.ForEach(z => z.SetIsWatchedAndNotify(!z.IsWatched));

        public void WatchNone() => this.Watcheds.ForEach(z => z.SetIsWatchedAndNotify(false));

        public async Task<bool> WatchSaveAsync()
        {
            var manager = this.GetManagers().VideoManager;
            var video = await manager.FindAsync(this.InfoView.Source.Id);
            if (video == null)
            {
                Debug.Assert(false);
                return false;
            }

            var watched = this.Watcheds.Where(z => z.IsWatched)
                .Select(z => z.Episode)
                .OrderBy(z => z)
                .ToList();
            if (watched.Count == 0)
            {
                if (video.Watcheds != null)
                {
                    video.Watcheds = null;
                    if (await manager.UpdateAsync(video))
                    {
                        this.ShowStatueMessage("watched updated.");
                        return true;
                    }
                }
            }
            else
            {
                if (video.Watcheds?.Count != watched.Count || !watched.SequenceEqual(video.Watcheds))
                {
                    video.Watcheds = watched;
                    if (await manager.UpdateAsync(video))
                    {
                        this.ShowStatueMessage("watched updated.");
                        return true;
                    }
                }
            }

            return false;
        }

        public JasilyCollectionView<ObservableCollectionGroup<string, EntityViewModel>> EntitesView { get; private set; }

        private int CompareEntityViewModel(EntityViewModel x, EntityViewModel y)
        {
            Debug.Assert(x != null, "x != null");
            Debug.Assert(y != null, "y != null");

            if (x.Source.Id == y.Source.Id) return 0;

            if (x.Source.Fansubs.Count > 0 && y.Source.Fansubs.Count > 0)
                return string.CompareOrdinal(x.Source.Fansubs[0], y.Source.Fansubs[0]);

            if (x.Source.Extension != y.Source.Extension)
                return string.CompareOrdinal(x.Source.Extension, y.Source.Extension);

            return -1;
        }

        public async void Flush()
        {
            if (await this.WatchSaveAsync())
            {
                this.InfoView.RefreshProperties();
            }

            this.Background.Flush();
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

        public sealed class BackgroundViewModel : HasCoverViewModel<ICoverParent>
        {
            private const double DefaultOpacity = 0.2;
            private readonly VideoViewerViewModel parent;
            private double opacity = DefaultOpacity;

            public BackgroundViewModel(VideoViewerViewModel parent)
                : base(parent.InfoView.Source.BackgroundImageAsCoverParent())
            {
                this.parent = parent;
            }

            public SeriesViewModel Series => this.parent.InfoView.SeriesView;

            public VideoInfoViewModel VideoInfo => this.parent.InfoView;

            protected override async Task<bool> TryAutoAddCoverAsync()
            {
                var client = JryVideoCore.Current.GetTheTVDBClient();
                if (client == null) return false;

                return (await this.AutoGenerateCoverAsync(client, this.VideoInfo.Source) ||
                        await this.AutoGenerateCoverOverTheTVDBIdAsync(client,
                            this.Series.Source.TheTVDBId, this.VideoInfo.Source.Index.ToString())) ||
                       await this.AutoGenerateCoverAsync(client, this.Series.Source);
            }

            private async Task<bool> AutoGenerateCoverAsync(TheTVDBClient client, IImdbItem item)
            {
                var imdbId = item.GetValidImdbId();
                if (imdbId == null) return false;

                foreach (var series in await client.GetSeriesByImdbIdAsync(imdbId))
                {
                    if (await this.AutoGenerateCoverOverTheTVDBIdAsync(client, series.SeriesId,
                        this.VideoInfo.Source.Index.ToString()))
                        return true;
                }
                return false;
            }

            private async Task<bool> AutoGenerateCoverOverTheTVDBIdAsync(TheTVDBClient client, string theTVDBId, string index)
            {
                if (theTVDBId.IsNullOrWhiteSpace()) return false;

                return await Task.Run(async () =>
                {
                    var array = (await client.GetBannersBySeriesIdAsync(theTVDBId)).ToArray();
                    foreach (var banner in array.Where(z => z.Season == index).RandomSort()
                        .Concat(array.Where(z => z.Season != index).RandomSort())
                        .Where(banner => banner.BannerType == BannerType.Fanart))
                    {
                        if (await this.DownloadAsync(banner.BuildUrl(client))) return true;
                    }
                    return false;
                });
            }

            private void Refresh() => base.BeginUpdateCover();

            public async void Reset()
            {
                await this.RemoveAsync();
                this.Refresh();
            }

            private async Task RemoveAsync()
            {
                var videoInfo = this.VideoInfo.Source;
                var bgId = videoInfo.BackgroundImageAsCoverParent().CoverId;
                await Task.Run(async () =>
                {
                    var coverManager = this.GetManagers().CoverManager;
                    if (await coverManager.RemoveAsync(bgId))
                    {
                        var videoManager = this.GetManagers().SeriesManager.GetVideoInfoManager(
                            this.VideoInfo.SeriesView.Source);
                        await videoManager.UpdateAsync(videoInfo);
                    }
                });
            }

            private async Task<bool> DownloadAsync(string url)
            {
                var builder = CoverBuilder.CreateBackground(this.VideoInfo.Source, url);
                return await this.GetManagers().CoverManager.BuildCoverAsync(builder);
            }

            public async Task<bool?> StartSelect(Window window)
            {
                var parameters = new List<RemoteId>();

                var imdbId = this.VideoInfo.Source.GetValidImdbId();
                if (imdbId != null)
                {
                    parameters.Add(new RemoteId(RemoteIdType.Imdb, imdbId));
                }

                if (!this.Series.Source.TheTVDBId.IsNullOrWhiteSpace())
                {
                    parameters.Add(new RemoteId(RemoteIdType.TheTVDB, this.Series.Source.TheTVDBId));
                }

                imdbId = this.Series.Source.GetValidImdbId();
                if (imdbId != null)
                {
                    parameters.Add(new RemoteId(RemoteIdType.Imdb, imdbId));
                }

                var url = WebImageSelectorWindow.StartSelectFanartByImdbId(window, this.VideoInfo.Source.Index.ToString(), parameters.ToArray());
                if (string.IsNullOrWhiteSpace(url)) return null;
                await this.RemoveAsync();
                await this.DownloadAsync(url);
                this.Refresh();
                return false;
            }

            protected override bool IsDelayLoad => true;

            public double Opacity
            {
                get { return this.opacity; }
                set { this.SetPropertyRef(ref this.opacity, value); }
            }

            public int? GetSaveValue()
            {
                var intValue = checked((int)(this.Opacity * 10));
                Debug.Assert(intValue >= 2 && intValue <= 8);
                return intValue < 3 || intValue > 8 ? (int?)null : intValue;
            }

            protected override void SetCover(JryCover cover)
            {
                if (cover?.Opacity != null)
                {
                    var o = (double)cover.Opacity.Value;
                    o = Math.Max(Math.Min(0.8, o / 10), 0.2);
                    this.Opacity = o;
                }
                else
                {
                    this.Opacity = DefaultOpacity;
                }
                base.SetCover(cover);
            }

            public async void Flush()
            {
                if (this.IsCoverEmpty) return;
                var manager = this.GetManagers().CoverManager;
                var cover = await manager.LoadCoverAsync(this.Source.CoverId);
                if (cover != null)
                {
                    var o = this.GetSaveValue(); ;
                    if (cover.Opacity != o)
                    {
                        cover.Opacity = o;
                        await manager.UpdateAsync(cover);
                    }
                }
            }
        }
    }
}