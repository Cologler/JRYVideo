using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Jasily;
using Jasily.ComponentModel;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
using JryVideo.Core.Models;
using JryVideo.Editors.CoverEditor;
using JryVideo.Editors.VideoEditor;
using JryVideo.Model;
using JryVideo.Model.Interfaces;
using JryVideo.Properties;
using static System.String;

namespace JryVideo.Common
{
    public sealed class VideoInfoViewModel : VideoInfoReadonlyViewModel
    {
        private static readonly RefreshPropertiesMapper Mapper = new RefreshPropertiesMapper(typeof(VideoInfoViewModel));

        public static event EventHandler<VideoInfoViewModel> IsWatchedUpdated;

        private bool isTrackButtonEnable;
        private bool isUntrackButtonEnable;
        private WatchedInfo todayPlaying;

        public VideoInfoViewModel(SeriesViewModel seriesViewModel, JryVideoInfo source)
            : base(source)
        {
            this.PropertiesMapper = Mapper;
            this.SeriesView = seriesViewModel;
            this.IsTrackButtonEnable = !(this.IsUntrackButtonEnable = this.Source.IsTracking);
            this.CoverViewModel.AutoGenerateCoverProvider = new AutoGenerateCoverProvider(this.GetManagers().CoverManager);
        }

        public SeriesViewModel SeriesView { get; }

        [NotifyPropertyChanged]
        public WatchedInfo TodayWatched => this.todayPlaying;

        [NotifyPropertyChanged]
        public bool IsEnableWatchedButton => this.TodayWatched?.Episode != null && !this.TodayWatched.IsWatched;

        [NotifyPropertyChanged]
        public Group VideoGroup { get; set; }

        public bool IsObsolete => this.SeriesView.IsObsolete;

        public override void RefreshProperties()
        {
            GroupFactory.RefreshGroup(this);
            base.RefreshProperties();
        }

        public bool NeedGroup { get; set; }

        private async void RefreshGroup(GroupFactory groupFactory)
        {
            // only tracking need build group info.
            if (!this.Source.IsTracking) return;

            int? episode;
            this.VideoGroup = groupFactory.GetGroup(this.Source, out episode);
            Debug.Assert(this.VideoGroup != null);

            if (this.VideoGroup.Mode == GroupMode.Today)
            {
                var playing = this.todayPlaying = new WatchedInfo(episode);
                if (playing.Episode.HasValue)
                {
                    var isWatched = await Task.Run(async () =>
                    {
                        var user = await this.GetManagers().UserWatchInfoManager.FindAsync(this.Source.Id);
                        return user.Watcheds?.Contains(playing.Episode.Value);
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

            base.RefreshProperties();
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
                    return JasilyNullable.Compare(y.Source.StartDate, x.Source.StartDate);
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

        private class AutoGenerateCoverProvider : IAutoGenerateCoverProvider
        {
            private readonly CoverManager manager;

            public AutoGenerateCoverProvider(CoverManager manager)
            {
                this.manager = manager;
            }

            /// <summary>
            /// return true if success.
            /// </summary>
            /// <returns></returns>
            public async Task<bool> GenerateAsync(DataCenter dataCenter, ICoverParent source)
            {
                var video = (JryVideoInfo)source;
                if (video.DoubanId == null) return false;

                return await Task.Run(async () =>
                {
                    var builder = CoverBuilder.CreateVideo(video);
                    var requests = (await DoubanHelper.TryGetMovieInfoAsync(video.DoubanId))?.GetMovieCoverRequest().ToArray();
                    if (requests == null) return false;
                    if (requests.Length == 0) return false;
                    builder.Requests.AddRange(requests);
                    return await this.manager.BuildCoverAsync(builder);
                });
            }
        }

        public static IEnumerable<VideoInfoViewModel> Create(JrySeries series)
            => new SeriesViewModel(series).VideoViewModels;

        public async Task<bool> TrackAsync()
        {
            this.IsTrackButtonEnable = this.IsUntrackButtonEnable = false;
            var manager = this.GetManagers().SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
            this.Source.IsTracking = true;

            if (await manager.UpdateAsync(this.Source))
            {
                this.IsUntrackButtonEnable = true;
                return true;
            }

            return false;
        }

        public async Task<bool> UntrackAndStarAsync(int star)
        {
            Debug.Assert(1 <= star && star <= 5);
            this.IsTrackButtonEnable = this.IsUntrackButtonEnable = false;
            var manager = this.GetManagers().SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
            this.Source.IsTracking = false;
            this.Source.Star = star;

            if (await manager.UpdateAsync(this.Source))
            {
                this.IsTrackButtonEnable = true;
                return true;
            }

            return false;
        }

        public async Task<bool> UntrackAsync()
        {
            this.IsTrackButtonEnable = this.IsUntrackButtonEnable = false;
            var manager = this.GetManagers().SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
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
            var manager = this.GetManagers().SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
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
            var manager = this.GetManagers().UserWatchInfoManager;
            var user = await manager.FindAsync(this.Source.Id);
            if (user == null) return;
            if (user.Watcheds == null) user.Watcheds = new List<int>();
            if (user.Watcheds.Contains(ep.Value)) return;
            user.Watcheds.Add(ep.Value);
            await manager.UpdateAsync(user);
            this.NotifyPropertyChanged(nameof(this.IsEnableWatchedButton));
        }

        public bool OpenEditorWindows(Window parent)
        {
            if (this.SeriesView.TestVersionObsolete(parent))
            {
                return false;
            }

            var dlg = new VideoEditorWindow(this)
            {
                Owner = parent
            };
            if (dlg.ShowDialog() == true)
            {
                GroupFactory.RefreshGroup(this);
                this.RefreshProperties();
                return true;
            }
            return false;
        }

        public async Task OpenCoverEditorWindows(Window parent)
        {
            if (this.SeriesView.TestVersionObsolete(parent))
            {
                return;
            }

            var viewModel = await CoverEditorViewModel.FromAsync(this.GetManagers().CoverManager, this.Source);
            viewModel.DoubanId = this.Source.DoubanId ?? string.Empty;
            viewModel.ImdbId = this.SeriesView.Source.ImdbId ?? this.Source.ImdbId ?? string.Empty;
            var dlg = new CoverEditorWindow(viewModel);

            if (dlg.ViewModel.DoubanId.IsNullOrWhiteSpace() && !this.Source.DoubanId.IsNullOrWhiteSpace())
            {
                dlg.ViewModel.DoubanId = this.Source.DoubanId;
            }

            if (dlg.ViewModel.ImdbId.IsNullOrWhiteSpace())
            {
                if (!this.SeriesView.Source.ImdbId.IsNullOrWhiteSpace())
                {
                    dlg.ViewModel.ImdbId = this.SeriesView.Source.ImdbId;
                }
                else if (!this.Source.ImdbId.IsNullOrWhiteSpace())
                {
                    dlg.ViewModel.ImdbId = this.Source.ImdbId;
                }
            }

            if (dlg.ShowDialog() == true)
            {
                await dlg.ViewModel.CommitAsync();
                this.CoverViewModel.RefreshProperties();
            }
        }

        public class GroupFactory
        {
            private static readonly Group[] Todays;
            private static readonly Group[] ThisWeeks;
            private static readonly Group[] NextWeeks;
            private static GroupFactory current;

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

                RebuildCurrent();
            }

            private static Group Today(DayOfWeek dayOfWeek) => Todays[(int)dayOfWeek];

            private static Group AllAired { get; } = new Group(GroupMode.AllAired, Resources.DateTime_AllAired);

            private static Group Unknown { get; } = new Group(GroupMode.Unknown, $"{Resources.DayOfWeek_Unknown} (unknown start)");

            private static Group ThisWeek(DayOfWeek dayOfWeek) => ThisWeeks[(int)dayOfWeek];

            private static Group NextWeek(DayOfWeek dayOfWeek) => NextWeeks[(int)dayOfWeek];

            private static Group FewWeek(int day, int week) => new Group(GroupMode.FewWeek, string.Format(Resources.DateTime_AfterWeek, week), day);

            private static Group FewMonth(int day, int month) => new Group(GroupMode.FewMonth, string.Format(Resources.DateTime_AfterMonth, month), day);

            private static Group Future(int day) => new Group(GroupMode.Future, Resources.DateTime_Future, day);

            private readonly DateTime today;
            private readonly DateTime nextSunday;
            private readonly DateTime nextNextSunday;

            private GroupFactory()
            {
                this.today = DateTime.Now.Date;
                var dayofWeekOffset = (int)DayOfWeek.Sunday - (int)this.today.DayOfWeek + 7;
                this.nextSunday = this.today.AddDays(dayofWeekOffset);
                this.nextNextSunday = this.nextSunday.AddDays(7);
            }

            public Group GetGroup(JryVideoInfo video, out int? episode)
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

            public static void RefreshGroup(VideoInfoViewModel video)
            {
                if (!video.NeedGroup) return;
                Debug.Assert(current != null);
                video.RefreshGroup(current);
            }

            public static void RebuildCurrent() => current = new GroupFactory();
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