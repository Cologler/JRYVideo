using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Core;
using JryVideo.Model;
using JryVideo.Properties;

namespace JryVideo.Common
{
    public sealed class VideoInfoViewModel : HasCoverViewModel<JryVideoInfo>
    {
        private string yearWithIndex;
        private bool isTrackButtonEnable;
        private bool isUntrackButtonEnable;
        private string dayOfWeek;
        private string todayEpisode;
        private bool isToday;

        public VideoInfoViewModel(JrySeries series, JryVideoInfo source)
            : base(source)
        {
            this.SeriesView = new SeriesViewModel(series);
            this.Reload();
        }

        public SeriesViewModel SeriesView { get; private set; }

        public string YearWithIndex
        {
            get { return this.yearWithIndex; }
            set { this.SetPropertyRef(ref this.yearWithIndex, value); }
        }

        [NotifyPropertyChanged]
        public string VideoNames => this.Source.Names.FirstOrDefault() ?? "";

        [NotifyPropertyChanged]
        public string VideoFullNames => this.Source.Names.Count == 0 ? null : this.Source.Names.AsLines();

        public string DayOfWeek
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
                if (this.Source.StartDate.HasValue)
                {
                    if (this.Source.StartDate.Value < DateTime.Now)
                    {
                        if (this.Source.DayOfWeek == DateTime.Now.DayOfWeek)
                        {
                            this.CompareMode = ViewModelCompareMode.Today;
                            this.DayOfWeek = String.Format("{0} ({1})", this.Source.DayOfWeek.GetLocalizeString(), Resources.DayOfWeek_Today);
                            var episode = this.Source.GetTodayEpisode(DateTime.Now);
                            this.TodayEpisode = episode <= this.Source.EpisodesCount
                                ? String.Format("today play {0}", episode)
                                : "done!";
                        }
                        else
                        {
                            this.CompareMode = ViewModelCompareMode.DayOfWeek;
                            this.DayOfWeek = this.Source.DayOfWeek.GetLocalizeString();
                            this.TodayEpisode = null;
                        }
                    }
                    else
                    {
                        this.CompareMode = ViewModelCompareMode.Future;
                        this.DayOfWeek = Resources.DateTime_Future;
                    }
                }
                else
                {
                    this.CompareMode = ViewModelCompareMode.Unknown;
                    this.DayOfWeek = String.Format("{0} ({1})", Resources.DayOfWeek_Unknown, "unknown start");
                }
            }
        }

        private ViewModelCompareMode CompareMode { get; set; }

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

                if (x.CompareMode != y.CompareMode)
                    return x.CompareMode.CompareTo(y.CompareMode);

                if (x.CompareMode == ViewModelCompareMode.DayOfWeek && x.Source.DayOfWeek != y.Source.DayOfWeek)
                {
                    if (x.Source.DayOfWeek == null) return -1;
                    if (y.Source.DayOfWeek == null) return 1;

                    var sub1 = ((int)x.Source.DayOfWeek) - ((int)this.DayOfWeek);
                    var sub2 = ((int)y.Source.DayOfWeek) - ((int)this.DayOfWeek);

                    return sub1 * sub2 > 0 ? sub1 - sub2 : sub2 - sub1;
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