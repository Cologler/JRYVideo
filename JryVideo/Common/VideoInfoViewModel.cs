using Jasily.ComponentModel;
using JryVideo.Core;
using JryVideo.Core.Douban;
using JryVideo.Editors.VideoEditor;
using JryVideo.Model;
using JryVideo.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static System.String;

namespace JryVideo.Common
{
    public sealed class VideoInfoViewModel : VideoInfoReadonlyViewModel
    {
        public static event EventHandler<VideoInfoViewModel> IsWatchedUpdated;

        private bool isTrackButtonEnable;
        private bool isUntrackButtonEnable;
        private WatchedInfo todayPlaying;

        public VideoInfoViewModel(SeriesViewModel seriesViewModel, JryVideoInfo source)
            : base(source)
        {
            this.SeriesView = seriesViewModel;
            this.RefreshProperties();
        }

        public SeriesViewModel SeriesView { get; }

        [NotifyPropertyChanged]
        public WatchedInfo TodayWatched => this.todayPlaying;

        [NotifyPropertyChanged]
        public bool IsEnableWatchedButton => this.TodayWatched?.Episode != null && !this.TodayWatched.IsWatched;

        [NotifyPropertyChanged]
        public Group VideoGroup { get; set; }

        public override void RefreshProperties()
        {
            this.IsTrackButtonEnable = !(this.IsUntrackButtonEnable = this.Source.IsTracking);
            this.UpdateGroup();
            base.RefreshProperties();
        }

        private async void UpdateGroup()
        {
            // only tracking need build group info.
            if (!this.Source.IsTracking) return;

            int? episode;
            this.VideoGroup = new GroupFactory().Build(this.Source, out episode);
            Debug.Assert(this.VideoGroup != null);

            if (this.VideoGroup.Mode == GroupMode.Today)
            {
                var playing = this.todayPlaying = new WatchedInfo(episode);
                if (playing.Episode.HasValue)
                {
                    var isWatched = await Task.Run(async () =>
                    {
                        var manager = JryVideoCore.Current.CurrentDataCenter.VideoManager;
                        var video = await manager.FindAsync(this.Source.Id);
                        return video?.Watcheds?.Contains(playing.Episode.Value);
                    });

                    if (isWatched != null && playing.IsWatched != isWatched.Value)
                    {
                        playing.IsWatched = isWatched.Value;
                        IsWatchedUpdated?.Invoke(this, this);
                        //this.NotifyPropertyChanged(nameof(this.TodayPlaying));
                        this.NotifyPropertyChanged(nameof(this.IsEnableWatchedButton));
                    }
                }
            }
            else
            {
                this.todayPlaying = null;
            }
        }

        internal sealed class GroupComparer : Comparer<VideoInfoViewModel>
        {
            public override int Compare(VideoInfoViewModel x, VideoInfoViewModel y)
            {
                Debug.Assert(x?.VideoGroup != null, "x != null");
                Debug.Assert(y?.VideoGroup != null, "y != null");

                var ret = x.VideoGroup.CompareTo(y.VideoGroup);
                if (ret == 0 && x.VideoGroup.Mode == GroupMode.Today)
                {
                    Debug.Assert(x.TodayWatched != null, "x.TodayPlaying != null");
                    Debug.Assert(y.TodayWatched != null, "y.TodayPlaying != null");
                    ret = x.TodayWatched.CompareTo(y.TodayWatched);
                }
                if (ret == 0 && x.VideoGroup.Mode == GroupMode.AllAired)
                {
                    Debug.Assert(x.Source.StartDate != null, "x.Source.StartDate != null");
                    Debug.Assert(y.Source.StartDate != null, "y.Source.StartDate != null");
                    ret = y.Source.StartDate.Value.CompareTo(x.Source.StartDate.Value);
                }
                return ret == 0 ? y.Source.Created.CompareTo(x.Source.Created) : ret;
            }
        }

        internal sealed class DefaultComparer : Comparer<VideoInfoViewModel>
        {
            public override int Compare(VideoInfoViewModel x, VideoInfoViewModel y)
            {
                Debug.Assert(x != null, "x != null");
                Debug.Assert(y != null, "y != null");

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

            var guid = await this.AutoGenerateCoverAsync();
            if (guid == null) return false;
            this.Source.CoverId = guid;
            var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
            return await manager.UpdateAsync(this.Source);
        }

        private async Task<string> AutoGenerateCoverAsync()
        {
            return await Task.Run(async () =>
            {
                var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;
                var cover = (await coverManager.Source.FindAsync(
                    new JryCover.QueryParameter()
                    {
                        CoverType = JryCoverType.Background,
                        VideoId = this.Source.Id
                    })).SingleOrDefault();
                if (cover != null) return cover.Id;
                if (this.Source.DoubanId == null) return null;
                var url = (await DoubanHelper.TryGetMovieInfoAsync(this.Source.DoubanId))?.GetLargeImageUrl();
                if (url == null) return null;
                cover = JryCover.CreateVideo(this.Source, url);
                return await coverManager.DownloadCoverAsync(cover);
            });
        }

        public static IEnumerable<VideoInfoViewModel> Create(JrySeries series)
            => new SeriesViewModel(series).VideoViewModels;

        public async Task<bool> TrackAsync()
        {
            this.IsTrackButtonEnable = this.IsUntrackButtonEnable = false;
            var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
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
            var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
            this.Source.IsTracking = false;

            if (await manager.UpdateAsync(this.Source))
            {
                this.IsTrackButtonEnable = true;
                return true;
            }

            return false;
        }

        public async Task<bool> AllAiredAsync()
        {
            if (this.Source.IsAllAired) return false;
            var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
            this.Source.IsAllAired = true;
            this.NotifyPropertyChanged(nameof(this.IsNotAllAired));
            return await manager.UpdateAsync(this.Source);
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

        public sealed class WatchedInfo : NotifyPropertyChangedObject, IComparable<WatchedInfo>
        {
            private const string WatchedBackgroundColor = "Blue";
            private const string UnwatchedBackgroundColor = "DarkOrange";

            private bool isWatched;
            public int? Episode { get; }

            public WatchedInfo(int? episode)
            {
                this.Episode = episode;
            }

            public string Text => this.Episode.HasValue ? $"today play {this.Episode.Value}" : "no more new!";

            public string WatchedText => $"watched ep {this.Episode}";

            public bool IsWatched
            {
                get { return this.isWatched; }
                set
                {
                    if (!this.SetPropertyRef(ref this.isWatched, value)) return;
                    this.NotifyPropertyChanged(nameof(this.BackgroundColor));
                }
            }

            public string BackgroundColor => this.IsWatched ? WatchedBackgroundColor : UnwatchedBackgroundColor;

            public int CompareTo(WatchedInfo other)
                => this.GetOrderHash().CompareTo(other.GetOrderHash());

            private int GetOrderHash() => this.Episode.HasValue ? (this.IsWatched ? 2 : 0) : 1;
        }

        public async void Watch()
        {
            var ep = this.TodayWatched?.Episode;
            if (ep == null) return;
            this.TodayWatched.IsWatched = true;
            var manager = JryVideoCore.Current.CurrentDataCenter.VideoManager;
            var video = await manager.FindAsync(this.Source.Id);
            if (video == null) return;
            if (video.Watcheds == null) video.Watcheds = new List<int>();
            if (video.Watcheds.Contains(ep.Value)) return;
            video.Watcheds.Add(ep.Value);
            await manager.UpdateAsync(video);
            this.NotifyPropertyChanged(nameof(this.IsEnableWatchedButton));
        }

        public bool OpenEditorWindows(Window parent)
        {
            var dlg = new VideoEditorWindow(this.SeriesView.Source, this.Source)
            {
                Owner = parent
            };
            if (dlg.ShowDialog() == true)
            {
                this.RefreshProperties();
                this.BeginUpdateCover();
                return true;
            }
            return false;
        }

        public class GroupFactory
        {
            private static readonly Group[] Todays;
            private static readonly Group[] ThisWeeks;
            private static readonly Group[] NextWeeks;

            static GroupFactory()
            {
                var dayOfWeeks = JasilyEnum.GetValues<DayOfWeek>();
                Todays = dayOfWeeks
                    .Select(z => new Group(GroupMode.Today, $"{z.GetLocalizeString()} ({Resources.DayOfWeek_Today})", z))
                    .ToArray();
                ThisWeeks = dayOfWeeks
                    .Select(z => new Group(GroupMode.ThisWeek, z.GetLocalizeString(), z))
                    .ToArray();
                NextWeeks = dayOfWeeks
                    .Select(z => new Group(GroupMode.NextWeek, string.Format(Resources.DateTime_Next, z.GetLocalizeString()), z))
                    .ToArray();
            }

            public static Group Today(DayOfWeek dayOfWeek) => Todays[(int)dayOfWeek];

            public static Group AllAired { get; } = new Group(GroupMode.AllAired, Resources.DateTime_AllAired);

            public static Group Unknown { get; } = new Group(GroupMode.Unknown, $"{Resources.DayOfWeek_Unknown} (unknown start)");

            public static Group ThisWeek(DayOfWeek dayOfWeek) => ThisWeeks[(int)dayOfWeek];

            public static Group NextWeek(DayOfWeek dayOfWeek) => NextWeeks[(int)dayOfWeek];

            public static Group FewWeek(int day, int week) => new Group(GroupMode.FewWeek, string.Format(Resources.DateTime_AfterWeek, week), day);

            public static Group FewMonth(int day, int month) => new Group(GroupMode.FewMonth, string.Format(Resources.DateTime_AfterMonth, month), day);

            public static Group Future(int day) => new Group(GroupMode.Future, Resources.DateTime_Future, day);

            private readonly DateTime today;
            private readonly DateTime nextSunday;
            private readonly DateTime nextNextSunday;

            public GroupFactory()
            {
                this.today = DateTime.Now.Date;
                var dayofWeekOffset = (int)DayOfWeek.Sunday - (int)this.today.DayOfWeek + 7;
                this.nextSunday = this.today.AddDays(dayofWeekOffset);
                this.nextNextSunday = this.nextSunday.AddDays(7);
            }

            public Group Build(JryVideoInfo video, out int? episode)
            {
                episode = null;
                if (video.IsAllAired) return AllAired;
                if (!video.StartDate.HasValue) return Unknown;

                var startDate = video.StartDate.Value.ToLocalTime().Date; // 第一集播出时间
                var dayOfWeek = video.DayOfWeek ?? startDate.DayOfWeek;
                if (startDate.DayOfWeek != dayOfWeek)
                {
                    var offset = (int)dayOfWeek - (int)startDate.DayOfWeek;
                    startDate = startDate.AddDays(offset < 0 ? offset + 7 : offset); // 修正偏移量后的播出时间
                }
                var nextAirDate = startDate; // 下一集播出时间
                if (nextAirDate <= this.today) // 已经播出
                {
                    var ep = video.GetTodayEpisode(this.today) + (video.EpisodeOffset ?? 0);
                    if (ep <= video.EpisodesCount)
                    {
                        episode = ep;
                    }

                    var offset = (int)dayOfWeek - (int)this.today.DayOfWeek;
                    nextAirDate = this.today.AddDays(offset < 0 ? offset + 7 : offset);
                }
                if (nextAirDate == this.today) return Today(dayOfWeek);
                if (nextAirDate < this.nextSunday) return ThisWeek(dayOfWeek);
                if (nextAirDate < this.nextNextSunday) return NextWeek(dayOfWeek);
                var dayOffset = (nextAirDate - this.today).Days;
                var week = (nextAirDate - this.nextSunday).Days / 7;
                if (week < 4) return FewWeek(dayOffset, week + 1);
                var month = (nextAirDate.Year - this.nextSunday.Year) * 12 + (nextAirDate.Month - this.nextSunday.Month);
                if (month < 10) return FewMonth(dayOffset, month);
                return Future(dayOffset);
            }
        }

        public class Group : IComparable<Group>
        {
            private readonly DayOfWeek dayOfWeek;
            private readonly int offset;

            public Group(GroupMode mode, string title, int offset = 0)
            {
                this.offset = offset;
                this.Mode = mode;
                this.Title = title;
            }

            public Group(GroupMode mode, string title, DayOfWeek dayOfWeek)
            {
                this.dayOfWeek = dayOfWeek;
                this.Mode = mode;
                this.Title = title;
            }

            public GroupMode Mode { get; }

            public string Title { get; }

            public int CompareTo(Group other)
            {
                if (this.Mode != other.Mode) return ((int)this.Mode).CompareTo((int)other.Mode);

                switch (this.Mode)
                {
                    case GroupMode.Today:
                    case GroupMode.AllAired:
                    case GroupMode.Unknown:
                        return 0;

                    case GroupMode.ThisWeek:
                    case GroupMode.NextWeek:
                        return this.dayOfWeek.CompareTo(other.dayOfWeek);

                    case GroupMode.FewWeek:
                    case GroupMode.FewMonth:
                    case GroupMode.Future:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return this.offset.CompareTo(other.offset);
            }
        }

        public enum GroupMode
        {
            Today,

            AllAired,

            Unknown,

            ThisWeek,

            NextWeek,

            FewWeek,

            FewMonth,

            Future
        }

        public VideoInfoViewModel TryFindLastViewModel()
            => this.HasLast
                ? this.SeriesView.VideoViewModels.FirstOrDefault(z => z.Source.Id == this.Source.LastVideoId)
                : null;

        public VideoInfoViewModel TryFindNextViewModel()
            => this.HasNext
                ? this.SeriesView.VideoViewModels.FirstOrDefault(z => z.Source.Id == this.Source.NextVideoId)
                : null;

        public async Task AutoCompleteAsync()
        {
            await this.SeriesView.AutoCompleteAsync();
        }
    }
}