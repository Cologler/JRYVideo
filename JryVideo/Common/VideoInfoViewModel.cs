using Jasily.ComponentModel;
using JryVideo.Core;
using JryVideo.Model;
using JryVideo.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static System.String;

namespace JryVideo.Common
{
    public sealed class VideoInfoViewModel : HasCoverViewModel<JryVideoInfo>
    {
        private bool isTrackButtonEnable;
        private bool isUntrackButtonEnable;
        private string dayOfWeek;
        private string todayEpisode;

        public VideoInfoViewModel(JrySeries series, JryVideoInfo source)
            : base(source)
        {
            this.SeriesView = new SeriesViewModel(series);
            this.Reload();
        }

        public SeriesViewModel SeriesView { get; private set; }

        [NotifyPropertyChanged]
        public string YearWithIndex => $"({this.Source.Year}) {this.Source.Index}";

        [NotifyPropertyChanged]
        public string VideoNames => this.Source.Names.FirstOrDefault() ?? Empty;

        [NotifyPropertyChanged]
        public string VideoFullNames => this.Source.Names.Count == 0 ? null : this.Source.Names.AsLines();

        public string GroupTitle
        {
            get { return this.dayOfWeek; }
            private set { this.SetPropertyRef(ref this.dayOfWeek, value); }
        }

        public string TodayEpisode
        {
            get { return this.todayEpisode; }
            private set { this.SetPropertyRef(ref this.todayEpisode, value); }
        }

        public override void Reload()
        {
            base.Reload();

            base.RefreshProperties();

            this.IsTrackButtonEnable = !(this.IsUntrackButtonEnable = this.Source.IsTracking);

            // only tracking need build group info.
            if (this.Source.IsTracking)
            {
                var today = DateTime.Now;
                if (this.Source.StartDate.HasValue)
                {
                    if (this.Source.StartDate.Value < today)
                    {
                        if (this.Source.DayOfWeek == today.DayOfWeek)
                        {
                            this.compareMode = ViewModelCompareMode.Today;
                            this.GroupTitle = $"{this.Source.DayOfWeek.GetLocalizeString()} ({Resources.DayOfWeek_Today})";
                            var episode = this.Source.GetTodayEpisode(today);
                            this.TodayEpisode = episode <= this.Source.EpisodesCount
                                ? $"today play {episode}"
                                : "done!";
                        }
                        else
                        {
                            this.compareMode = ViewModelCompareMode.DayOfWeek;
                            this.GroupTitle = this.Source.DayOfWeek.GetLocalizeString();
                            this.TodayEpisode = null;
                        }
                    }
                    else
                    {
                        this.compareMode = ViewModelCompareMode.Future;
                        this.GroupTitle = Resources.DateTime_Future;
                    }
                }
                else
                {
                    this.compareMode = ViewModelCompareMode.Unknown;
                    this.GroupTitle = $"{Resources.DayOfWeek_Unknown} (unknown start)";
                }
            }
        }

        private ViewModelCompareMode compareMode;

        private enum ViewModelCompareMode
        {
            Today,

            DayOfWeek,

            Future,

            Unknown
        }

        internal sealed class DayOfWeekComparer : Comparer<VideoInfoViewModel>
        {
            private readonly DayOfWeek DayOfWeek = DateTime.Now.DayOfWeek;

            public override int Compare(VideoInfoViewModel x, VideoInfoViewModel y)
            {
                Debug.Assert(x != null, "x != null");
                Debug.Assert(y != null, "y != null");

                if (x.compareMode != y.compareMode)
                    return x.compareMode.CompareTo(y.compareMode);

                if (x.compareMode == ViewModelCompareMode.DayOfWeek && x.Source.DayOfWeek != y.Source.DayOfWeek)
                {
                    if (x.Source.DayOfWeek == null) return -1;
                    if (y.Source.DayOfWeek == null) return 1;

                    var sub1 = ((int)x.Source.DayOfWeek) - ((int)this.DayOfWeek);
                    var sub2 = ((int)y.Source.DayOfWeek) - ((int)this.DayOfWeek);

                    return sub1 * sub2 > 0 ? sub1 - sub2 : sub2 - sub1;
                }

                if (x.compareMode == ViewModelCompareMode.Future && x.Source.StartDate != y.Source.StartDate)
                {
                    if (x.Source.StartDate == null) return -1;
                    if (y.Source.StartDate == null) return 1;

                    return x.Source.StartDate.Value.CompareTo(y.Source.StartDate.Value);
                }

                return y.Source.Created.CompareTo(x.Source.Created);
            }
        }

        internal sealed class DefaultComparer : Comparer<VideoInfoViewModel>
        {
            public override int Compare(VideoInfoViewModel x, VideoInfoViewModel y)
            {
                Debug.Assert(x != null, "x != null");
                Debug.Assert(y != null, "y != null");

                if (x.SeriesView.DisplayNameFirstLine == "火星救援")
                {
                    
                }

                if (x.SeriesView.Source.Id != y.SeriesView.Source.Id)
                {
                    return y.SeriesView.Source.Created.CompareTo(x.SeriesView.Source.Created);
                }

                // in same video: 1. date 1. year 1. type 1. index
                if (x.Source.StartDate.HasValue && y.Source.StartDate.HasValue)
                {
                    return y.Source.StartDate.Value.CompareTo(x.Source.StartDate.Value);
                }

                if (x.Source.Year != y.Source.Year)
                {
                    return y.Source.Year - x.Source.Year;
                }

                if (x.Source.Type != y.Source.Type)
                {
                    return CompareOrdinal(x.Source.Type, y.Source.Type);
                }

                if (x.Source.Index != y.Source.Index)
                {
                    return x.Source.Index - y.Source.Index;
                }

                return y.Source.Created.CompareTo(x.Source.Created);
            }
        }

        internal void NavigateToDouban()
        {
            var doubanId = this.Source.DoubanId;
            Task.Run(() =>
            {
                if (!doubanId.IsNullOrWhiteSpace())
                {
                    using (Process.Start("http://movie.douban.com/subject/" + doubanId))
                    {
                    }
                }
            });
        }

        internal void NavigateToImdb()
        {
            var imdnId = this.Source.ImdbId;
            Task.Run(() =>
            {
                if (!imdnId.IsNullOrWhiteSpace())
                {
                    using (Process.Start("http://www.imdb.com/title/" + imdnId))
                    {
                    }
                }
            });
        }

        protected override async Task<bool> TryAutoAddCoverAsync()
        {
            if (this.Source.DoubanId == null) return false;

            var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;

            var guid = await coverManager.GetCoverFromDoubanIdAsync(JryCoverType.Video, this.Source.DoubanId);

            if (guid == null) return false;

            this.Source.CoverId = guid;

            var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
            return await manager.UpdateAsync(this.Source);
        }

        public static IEnumerable<VideoInfoViewModel> Create(JrySeries series)
        {
            return series.Videos.Select(jryVideo => new VideoInfoViewModel(series, jryVideo));
        }

        public async Task<bool> TrackAsync()
        {
            this.IsTrackButtonEnable = this.IsUntrackButtonEnable = false;
            var manager =
                JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
            this.Source.IsTracking = true;

            if (await manager.UpdateAsync(this.Source))
            {
                this.IsUntrackButtonEnable = true;
                return true;
            }

            return false;
        }

        public async Task<bool> UntrackAsync()
        {
            this.IsTrackButtonEnable = this.IsUntrackButtonEnable = false;
            var manager =
                JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
            this.Source.IsTracking = false;

            if (await manager.UpdateAsync(this.Source))
            {
                this.IsTrackButtonEnable = true;
                return true;
            }

            return false;
        }

        public bool IsTrackButtonEnable
        {
            get { return this.isTrackButtonEnable; }
            set { this.SetPropertyRef(ref this.isTrackButtonEnable, value); }
        }

        public bool IsUntrackButtonEnable
        {
            get { return this.isUntrackButtonEnable; }
            set { this.SetPropertyRef(ref this.isUntrackButtonEnable, value); }
        }
    }
}