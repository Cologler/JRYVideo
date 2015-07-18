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
        public string VideoNames
        {
            get { return this.Source.Names.FirstOrDefault() ?? ""; }
        }

        [NotifyPropertyChanged]
        public string VideoFullNames
        {
            get { return this.Source.Names.AsLines(); }
        }

        public string DayOfWeek
        {
            get { return this.dayOfWeek; }
            private set { this.SetPropertyRef(ref this.dayOfWeek, value); }
        }

        public bool IsToday
        {
            get { return this.isToday; }
            private set { this.SetPropertyRef(ref this.isToday, value); }
        }

        public string TodayEpisode
        {
            get { return this.todayEpisode; }
            private set { this.SetPropertyRef(ref this.todayEpisode, value); }
        }

        [NotifyPropertyChanged]
        public bool IsEnterDoubanButtonEnable
        {
            get { return !this.Source.DoubanId.IsNullOrWhiteSpace(); }
        }

        public override void Reload()
        {
            base.Reload();

            base.RefreshProperties();

            this.IsTrackButtonEnable = !(this.IsUntrackButtonEnable = this.Source.IsTracking);

            if (!this.Source.StartDate.HasValue || this.Source.StartDate.Value < DateTime.Now)
            {
                this.IsToday = this.Source.DayOfWeek == DateTime.Now.DayOfWeek;

                this.DayOfWeek = this.IsToday
                    ? String.Format("{0} ({1})",
                        this.Source.DayOfWeek.GetLocalizeString(),
                        Resources.DayOfWeek_Today)
                    : this.Source.DayOfWeek.GetLocalizeString();

                if (this.IsToday)
                {
                    var episode = this.Source.GetTodayEpisode(DateTime.Now);

                    this.TodayEpisode = this.IsToday && episode <= this.Source.EpisodesCount
                        ? episode <= this.Source.EpisodesCount
                            ? String.Format("today play {0}", episode)
                            : "done!"
                        : null;
                }
            }
            else
            {
                this.DayOfWeek = Resources.DateTime_Future;
            }
        }

        public void EnterDouban()
        {
            var douban = this.Source.DoubanId;
            if (!douban.IsNullOrWhiteSpace())
            {
                using (Process.Start("http://movie.douban.com/subject/" + douban))
                {
                }
            }
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