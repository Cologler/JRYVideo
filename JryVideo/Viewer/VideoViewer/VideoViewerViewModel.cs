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
using JryVideo.Core.Managers;
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
            this.Background = new BackgroundImageViewModel(this);
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

        public BackgroundImageViewModel Background { get; }

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

            this.Background.SaveOpacity();
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

        public sealed class BackgroundImageViewModel : CoverViewModel
        {
            private readonly VideoViewerViewModel videoViewer;
            private const int OpacityMax = 8;
            private const int OpacityMin = 2;
            private const int DefaultOpacity = OpacityMin;
            private const double DefaultOpacityDouble = (double)DefaultOpacity / 10;
            private double opacity = DefaultOpacityDouble;

            public BackgroundImageViewModel(VideoViewerViewModel source)
                : base(source.InfoView.Source.BackgroundImageAsCoverParent())
            {
                this.videoViewer = source;
                this.AutoGenerateCoverProvider = new BackgroundAutoGenerateCoverProvider(source);
                this.IsDelayLoad = true;
            }

            public double Opacity
            {
                get { return this.opacity; }
                set { this.SetPropertyRef(ref this.opacity, value); }
            }

            public int? OpacityData
            {
                get
                {
                    var intValue = checked((int)(this.Opacity * 10));
                    intValue = Math.Max(OpacityMin, Math.Min(OpacityMax, intValue));
                    return intValue == DefaultOpacity ? (int?)null : intValue;
                }
                set
                {
                    if (!value.HasValue) return;
                    var o = (double)value;
                    o = Math.Max(Math.Min((double)OpacityMax / 10, o / 10), (double)OpacityMin / 10);
                    this.Opacity = o;
                }
            }

            public async void SaveOpacity()
            {
                var manager = this.GetManagers().CoverManager;
                var cover = await manager.LoadCoverAsync(this.Source.CoverId);
                if (cover == null) return;
                {
                    var o = this.OpacityData;
                    if (cover.Opacity != o)
                    {
                        cover.Opacity = o;
                        await manager.UpdateAsync(cover);
                    }
                }
            }

            protected override void OnLoadCoverEnd(JryCover cover)
            {
                this.OpacityData = cover?.Opacity;
                base.OnLoadCoverEnd(cover);
            }

            public async void Reset()
            {
                await this.AutoGenerateCoverProvider.GenerateAsync(this.GetManagers(), this.Source);
                this.RefreshProperties();
            }

            private async Task RemoveAsync()
            {
                var bgId = this.Source.CoverId;
                if (bgId == null) return;
                await this.GetManagers().CoverManager.RemoveAsync(bgId);
            }

            public async Task<bool?> StartSelect(Window window)
            {
                var parameters = new List<RemoteId>();

                var imdbId = this.videoViewer.InfoView.Source.GetValidImdbId();
                if (imdbId != null)
                {
                    parameters.Add(new RemoteId(RemoteIdType.Imdb, imdbId));
                }

                var theTVDBId = this.videoViewer.InfoView.SeriesView.Source.TheTVDBId;
                if (!theTVDBId.IsNullOrWhiteSpace())
                {
                    parameters.Add(new RemoteId(RemoteIdType.TheTVDB, theTVDBId));
                }

                imdbId = this.videoViewer.InfoView.SeriesView.Source.GetValidImdbId();
                if (imdbId != null)
                {
                    parameters.Add(new RemoteId(RemoteIdType.Imdb, imdbId));
                }

                var url = WebImageSelectorWindow.StartSelectFanartByImdbId(window,
                    this.videoViewer.InfoView.Source.Index.ToString(), parameters.ToArray());
                if (string.IsNullOrWhiteSpace(url)) return null;
                var builder = CoverBuilder.CreateBackground(this.videoViewer.InfoView.Source);
                builder.Uri.Add(url);
                await this.GetManagers().CoverManager.BuildCoverAsync(builder);
                this.RefreshProperties();
                return false;
            }
        }

        public class BackgroundAutoGenerateCoverProvider : IAutoGenerateCoverProvider
        {
            private readonly VideoViewerViewModel source;
            private CoverManager coverManager;

            public BackgroundAutoGenerateCoverProvider(VideoViewerViewModel source)
            {
                this.source = source;
            }

            /// <summary>
            /// return true if success.
            /// </summary>
            /// <param name="dataCenter"></param>
            /// <param name="source"></param>
            /// <returns></returns>
            public async Task<bool> GenerateAsync(DataCenter dataCenter, ICoverParent source)
            {
                var client = JryVideoCore.Current.GetTheTVDBClient();
                if (client == null) return false;

                this.coverManager = dataCenter.CoverManager;
                return await this.AutoGenerateCoverAsync(client, this.source.InfoView.Source) ||
                       await this.AutoGenerateCoverOverTheTVDBIdAsync(client,
                           this.source.InfoView.SeriesView.Source.TheTVDBId,
                           this.source.InfoView.Source.Index.ToString()) ||
                       await this.AutoGenerateCoverAsync(client, this.source.InfoView.SeriesView.Source);
            }

            private async Task<bool> AutoGenerateCoverAsync(TheTVDBClient client, IImdbItem item)
            {
                var imdbId = item.GetValidImdbId();
                if (imdbId == null) return false;

                foreach (var series in await client.GetSeriesByImdbIdAsync(imdbId))
                {
                    if (await this.AutoGenerateCoverOverTheTVDBIdAsync(client, series.SeriesId,
                        this.source.InfoView.Source.Index.ToString()))
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
                    var urls = array.Where(z => z.Season == index).RandomSort()
                        .Concat(array.Where(z => z.Season != index).RandomSort())
                        .Where(banner => banner.BannerType == BannerType.Fanart)
                        .Select(z => z.BuildUrl(client))
                        .ToArray();
                    if (urls.Length == 0) return false;
                    var builder = CoverBuilder.CreateBackground(this.source.InfoView.Source);
                    builder.Uri.AddRange(urls);
                    return await this.coverManager.BuildCoverAsync(builder);
                });
            }
        }
    }
}