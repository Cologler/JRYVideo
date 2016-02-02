﻿using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.TheTVDB;
using JryVideo.Model;
using JryVideo.Selectors.WebImageSelector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace JryVideo.Viewer.VideoViewer
{
    public sealed class VideoViewerViewModel : JasilyViewModel
    {
        private VideoViewModel video;
        private BackgroundViewModel background;
        private VideoRoleCollectionViewModel videoRoleCollection;

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
            private set { this.SetPropertyRef(ref this.background, value); }
        }

        public VideoRoleCollectionViewModel VideoRoleCollection
        {
            get { return this.videoRoleCollection; }
            private set { this.SetPropertyRef(ref this.videoRoleCollection, value); }
        }

        public async Task LoadAsync()
        {
            this.Background = new BackgroundViewModel(new BackgroundCover(this));
            this.videoRoleCollection = new VideoRoleCollectionViewModel(this.InfoView.SeriesView.Source, this.InfoView.Source)
            {
                VideoViewerViewModel = this
            };
            await this.ReloadVideoAsync();
            this.videoRoleCollection.BeginLoad();
            await this.AutoCompleteAsync();
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

            await JryVideoCore.Current.CurrentDataCenter.VideoRoleManager.AutoCreateVideoRoleOnInitialize(this.InfoView.Source);
            await JryVideoCore.Current.CurrentDataCenter.VideoRoleManager.AutoCreateVideoRoleOnInitialize(this.InfoView.SeriesView.Source);
        }

        public async Task AutoCompleteAsync()
        {
            await this.InfoView.AutoCompleteAsync();
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

            private async Task<bool> SetBackgroundIdAsync(string coverId)
                => await this.Source.UpdateImageIfEmptyAsync(coverId);

            protected override async Task<bool> TryAutoAddCoverAsync()
            {
                if (await this.TrySetByExistsAsync()) return true;

                var client = JryVideoCore.Current.TheTVDBClient;
                if (client == null) return false;

                var guid = (await this.AutoGenerateCoverAsync(client, this.Source.VideoInfo.Source) ??
                            await this.AutoGenerateCoverOverTheTVDBIdAsync(client, this.Source.Series.Source.TheTVDBId, this.Source.Index.ToString())) ??
                           await this.AutoGenerateCoverAsync(client, this.Source.Series.Source);

                if (guid != null)
                {
                    return await this.SetBackgroundIdAsync(guid);
                }

                return false;
            }

            private async Task<bool> TrySetByExistsAsync()
            {
                if (this.Source.ImdbId == null || !this.Source.ImdbId.StartsWith("tt")) return false;
                var cover = (await this.GetManagers().CoverManager.Source.FindAsync(
                    new JryCover.QueryParameter()
                    {
                        CoverType = JryCoverType.Background,
                        VideoId = this.Source.Source.InfoView.Source.Id
                    })).SingleOrDefault();
                if (cover == null) return false;
                return await this.SetBackgroundIdAsync(cover.Id);
            }

            private async Task<string> AutoGenerateCoverAsync(TheTVDBClient client, IImdbItem item)
            {
                var imdbId = item.GetValidImdbId();
                if (imdbId == null) return null;

                foreach (var series in await client.GetSeriesByImdbIdAsync(imdbId))
                {
                    var guid = await this.AutoGenerateCoverOverTheTVDBIdAsync(client, series.SeriesId, this.Source.Index.ToString());
                    if (guid != null) return guid;
                }
                return null;
            }

            private async Task<string> AutoGenerateCoverOverTheTVDBIdAsync(TheTVDBClient client, string theTVDBId, string index)
            {
                if (theTVDBId.IsNullOrWhiteSpace()) return null;

                return await Task.Run(async () =>
                {
                    var array = (await client.GetBannersBySeriesIdAsync(theTVDBId)).ToArray();
                    foreach (var banner in array.Where(z => z.Season == index).RandomSort()
                        .Concat(array.Where(z => z.Season != index).RandomSort())
                        .Where(banner => banner.BannerType == BannerType.Fanart))
                    {
                        var guid = await this.DownloadAsync(banner.BuildUrl(client));
                        if (guid != null) return guid;
                    }
                    return null;
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
                var videoInfo = this.Source.Source.InfoView.Source;
                var bgId = videoInfo.BackgroundImageId;
                if (bgId == null) return;
                await Task.Run(async () =>
                {
                    var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;
                    if (await coverManager.RemoveAsync(bgId))
                    {
                        var videoManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(
                            this.Source.Source.InfoView.SeriesView.Source);
                        videoInfo.BackgroundImageId = null;
                        await videoManager.UpdateAsync(videoInfo);
                    }
                });
            }

            private async Task<string> DownloadAsync(string url)
            {
                var cover = JryCover.CreateBackground(this.Source.Source.InfoView.Source, url);
                return await JryVideoCore.Current.CurrentDataCenter.CoverManager.DownloadCoverAsync(cover);
            }

            public async Task<bool?> StartSelect(Window window)
            {
                var parameters = new List<RemoteId>();

                var imdbId = this.Source.VideoInfo.Source.GetValidImdbId();
                if (imdbId != null)
                {
                    parameters.Add(new RemoteId(RemoteIdType.Imdb, imdbId));
                }

                if (!this.Source.Series.Source.TheTVDBId.IsNullOrWhiteSpace())
                {
                    parameters.Add(new RemoteId(RemoteIdType.TheTVDB, this.Source.Series.Source.TheTVDBId));
                }

                imdbId = this.Source.Series.Source.GetValidImdbId();
                if (imdbId != null)
                {
                    parameters.Add(new RemoteId(RemoteIdType.Imdb, imdbId));
                }

                var url = WebImageSelectorWindow.StartSelectFanartByImdbId(window, this.Source.Index.ToString(), parameters.ToArray());
                if (string.IsNullOrWhiteSpace(url)) return null;
                await this.RemoveAsync();
                await this.DownloadAsync(url);
                this.Refresh();
                return false;
            }
        }

        public sealed class BackgroundCover : IJryCoverParent
        {
            public VideoViewerViewModel Source { get; }

            public BackgroundCover(VideoViewerViewModel source)
            {
                this.Source = source;
            }

            public string CoverId
            {
                get { return this.Source.InfoView.Source.BackgroundImageId; }
                set { this.Source.InfoView.Source.BackgroundImageId = value; }
            }

            public SeriesViewModel Series => this.Source.InfoView.SeriesView;

            public VideoInfoViewModel VideoInfo => this.Source.InfoView;

            public string ImdbId => this.Source.InfoView.Source.ImdbId;

            public string SeriesImdbId => this.Source.InfoView.SeriesView.Source.ImdbId;

            public string DoubanId => this.Source.InfoView.Source.DoubanId;

            public int Index => this.Source.InfoView.Source.Index;

            public async Task<bool> UpdateImageIfEmptyAsync(string guid)
            {
                this.Source.InfoView.Source.BackgroundImageId = guid;
                var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(
                    this.Source.InfoView.SeriesView.Source);
                return await manager.UpdateAsync(this.Source.InfoView.Source);
            }
        }
    }
}