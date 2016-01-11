using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Common;
using JryVideo.Core;
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

        public async Task LoadAsync()
        {
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
    }
}