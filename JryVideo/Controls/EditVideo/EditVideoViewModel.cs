using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Enums;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Jasily.ComponentModel.Editable;
using JryVideo.Common;
using JryVideo.Common.ValidationRules;
using JryVideo.Controls.SelectFlag;
using JryVideo.Core.Douban;
using JryVideo.Editors.CoverEditor;
using JryVideo.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using static JryVideo.Common.Helper;

namespace JryVideo.Controls.EditVideo
{
    public class EditVideoViewModel : EditorItemViewModel<JryVideoInfo>
    {
        private ImageViewModel imageViewModel;
        private string selectedType;
        private CoverEditorViewModel cover;
        private string year;
        private string index;
        private string imdbId;
        private string doubanId;
        private string episodesCount;
        private bool isTracking;
        private bool isAllAired;
        private int episodeOffset;
        private NameValuePair<string, DayOfWeek?>? dayOfWeek;
        private DateTime? startDate;
        private VideoInfoReadonlyViewModel lastVideoViewModel;
        private VideoInfoReadonlyViewModel nextVideoViewModel;
        private int star;

        public EditVideoViewModel()
        {
            this.TypeCollection = new ObservableCollection<string>();
            this.YearCollection = new ObservableCollection<string>();
            this.IndexCollection = new ObservableCollection<string>();
            this.EpisodesCountCollection = new ObservableCollection<string>();
            this.DayOfWeekCollection = new ObservableCollection<NameValuePair<string, DayOfWeek?>>();
        }

        public ImageViewModel ImageViewModel
        {
            get { return this.imageViewModel; }
            private set { this.SetPropertyRef(ref this.imageViewModel, value); }
        }

        public ObservableCollection<string> TypeCollection { get; private set; }

        public ObservableCollection<string> YearCollection { get; private set; }

        public ObservableCollection<string> IndexCollection { get; private set; }

        public ObservableCollection<string> EpisodesCountCollection { get; private set; }

        public ObservableCollection<NameValuePair<string, DayOfWeek?>> DayOfWeekCollection { get; private set; }

        [EditableField]
        public string Type
        {
            get { return this.selectedType; }
            set { this.SetPropertyRef(ref this.selectedType, value); }
        }

        public string Year
        {
            get { return this.year; }
            set { this.SetPropertyRef(ref this.year, value); }
        }

        [EditableField]
        public string DoubanId
        {
            get { return this.doubanId; }
            set
            {
                value = TryGetDoubanId(value);
                this.SetPropertyRef(ref this.doubanId, value);
            }
        }

        [EditableField]
        public string ImdbId
        {
            get { return this.imdbId; }
            set
            {
                value = TryGetImdbId(value);
                this.SetPropertyRef(ref this.imdbId, value);
            }
        }

        [EditableField]
        public bool IsTracking
        {
            get { return this.isTracking; }
            set { this.SetPropertyRef(ref this.isTracking, value); }
        }

        [EditableField]
        public bool IsAllAired
        {
            get { return this.isAllAired; }
            set { this.SetPropertyRef(ref this.isAllAired, value); }
        }

        [EditableField]
        public int Star
        {
            get { return this.star; }
            set { this.SetPropertyRef(ref this.star, value); }
        }

        [EditableField]
        public DateTime? StartLocalDate
        {
            get { return this.startDate; }
            set
            {
                if (value.HasValue && this.Action == ObjectChangedAction.Create)
                {
                    if (this.DayOfWeek == null)
                    {
                        var offset = ((App)Application.Current).UserConfig?.AutoDayOfWeekOffset ?? 1;
                        this.DayOfWeek = this.GetDayOfWeekValue(offset == 0
                            ? value.Value.DayOfWeek
                            : (DayOfWeek)(((int)value.Value.DayOfWeek + offset) % 7));
                    }
                    var today = DateTime.Today;
                    if (value.Value >= today.AddDays(-(int)today.DayOfWeek)) // this week
                    {
                        this.IsTracking = true;
                    }
                    else if (this.EpisodesCount == "1")
                    {
                        this.IsAllAired = true;
                    }
                }
                this.SetPropertyRef(ref this.startDate, value);
            }
        }

        public SelectFlagViewModel TagsViewModel { get; } = new SelectFlagViewModel(JryFlagType.VideoTag);

        public VideoInfoReadonlyViewModel LastVideoViewModel
        {
            get { return this.lastVideoViewModel; }
            private set { this.SetPropertyRef(ref this.lastVideoViewModel, value, nameof(this.LastVideoViewModel), nameof(this.LastVideoName)); }
        }

        public string LastVideoName => GetContextVideoName(this.LastVideoViewModel);

        public VideoInfoReadonlyViewModel NextVideoViewModel
        {
            get { return this.nextVideoViewModel; }
            private set { this.SetPropertyRef(ref this.nextVideoViewModel, value, nameof(this.NextVideoViewModel), nameof(this.NextVideoName)); }
        }

        public string NextVideoName => GetContextVideoName(this.NextVideoViewModel);

        private static string GetContextVideoName(VideoInfoReadonlyViewModel video) => video == null
            ? "None"
            : (video.Source.Names.FirstOrDefault() ?? $"({video.Source.Year}) {video.Source.Type} {video.Source.Index}");

        public void ChangeContextVideo(bool isLast, JryVideoInfo video)
        {
            var vm = video != null ? new VideoInfoReadonlyViewModel(video) : null;
            if (isLast)
            {
                this.LastVideoViewModel = vm;
            }
            else
            {
                this.NextVideoViewModel = vm;
            }
        }

        public NameValuePair<string, DayOfWeek?>? DayOfWeek
        {
            get { return this.dayOfWeek; }
            set { this.SetPropertyRef(ref this.dayOfWeek, value); }
        }

        public SeriesViewModel Parent { get; set; }

        /// <summary>
        /// set from cover editor. if never use that, should be null.
        /// </summary>
        public CoverEditorViewModel Cover
        {
            get { return this.cover; }
            set { this.ImageViewModel = (this.cover = value) != null ? value.ImageViewModel : null; }
        }

        public string Index
        {
            get { return this.index; }
            set { this.SetPropertyRef(ref this.index, value); }
        }

        public NameEditableViewModel<JryVideoInfo> NamesViewModel { get; }
            = new NameEditableViewModel<JryVideoInfo>(false);

        public string EpisodesCount
        {
            get { return this.episodesCount; }
            set { this.SetPropertyRef(ref this.episodesCount, value); }
        }

        public int EpisodeOffset
        {
            get { return this.episodeOffset; }
            set { this.SetPropertyRef(ref this.episodeOffset, value); }
        }

        public void LoadFromDouban()
        {
            if (this.DoubanId.IsNullOrWhiteSpace()) return;
            this.BeginLoadDoubanMeta(this.DoubanId);
            this.BeginLoadDoubanHtml(this.DoubanId);
        }

        private async void BeginLoadDoubanMeta(string doubanId)
        {
            var info = await DoubanHelper.TryGetMovieInfoAsync(doubanId);
            if (info != null)
            {
                var parser = DoubanMovieParser.Parse(info);
                this.NamesViewModel.AddRange(parser.EntityNames);

                var defaultValue = (Application.Current as App)?.UserConfig?.DefaultValue;
                if (defaultValue != null)
                {
                    if (parser.IsMovie)
                    {
                        if (!defaultValue.MovieType.IsNullOrWhiteSpace())
                        {
                            this.Type = defaultValue.MovieType;
                        }
                    }
                    else
                    {
                        if (!defaultValue.SeasonType.IsNullOrWhiteSpace())
                        {
                            this.Type = defaultValue.SeasonType;
                        }
                    }
                }

                if (this.Year.IsNullOrWhiteSpace())
                {
                    this.Year = info.Year;
                }

                if (this.Index.IsNullOrWhiteSpace())
                {
                    this.Index = info.CurrentSeason ?? parser.Index;
                }

                if (this.EpisodesCount.IsNullOrWhiteSpace())
                {
                    this.EpisodesCount = parser.EpisodesCount ?? string.Empty;
                }
            }
        }

        private async void BeginLoadDoubanHtml(string doubanId)
        {
            if (this.ImdbId.IsNullOrWhiteSpace() || this.StartLocalDate == null)
            {
                var html = await DoubanHelper.TryGetMovieHtmlAsync(doubanId);
                if (html == null) return;

                if (this.ImdbId.IsNullOrWhiteSpace())
                {
                    this.ImdbId = DoubanHelper.TryParseImdbId(html) ?? string.Empty;
                }

                if (this.StartLocalDate == null)
                {
                    this.StartLocalDate = DoubanHelper.TryParseReleaseDate(html);
                }
            }
        }

        private NameValuePair<string, DayOfWeek?> GetDayOfWeekValue(DayOfWeek? dow)
            => this.DayOfWeekCollection.First(z => z.Value == dow);

        public override void WriteToObject(JryVideoInfo obj)
        {
            base.WriteToObject(obj);

            obj.Year = int.Parse(this.Year);
            obj.Index = int.Parse(this.Index);
            obj.EpisodesCount = int.Parse(this.EpisodesCount);
            this.NamesViewModel.WriteToObject(obj);
            obj.DayOfWeek = this.DayOfWeek?.Value;
            obj.EpisodeOffset = this.EpisodeOffset == 0 ? (int?)null : this.EpisodeOffset;

            var series = this.Parent.ThrowIfNull().Source;
            var lastVideoId = this.LastVideoViewModel?.Source.Id;
            if (obj.LastVideoId != lastVideoId)
            {
                obj.LastVideoId = lastVideoId;
                if (lastVideoId != null && obj.Id != null)
                {
                    var lastVideo = series.Videos.FirstOrDefault(z => z.Id == lastVideoId);
                    if (lastVideo != null && lastVideo.NextVideoId == null) lastVideo.NextVideoId = obj.Id;
                }
            }
            var nextVideoId = this.NextVideoViewModel?.Source.Id;
            if (obj.NextVideoId != nextVideoId)
            {
                obj.NextVideoId = nextVideoId;
                if (nextVideoId != null && obj.Id != null)
                {
                    var nextVideo = series.Videos.FirstOrDefault(z => z.Id == nextVideoId);
                    if (nextVideo != null && nextVideo.NextVideoId == null) nextVideo.LastVideoId = obj.Id;
                }
            }

            this.TagsViewModel.WriteTags(obj, true);
        }

        public override void ReadFromObject(JryVideoInfo obj)
        {
            base.ReadFromObject(obj);

            this.Index = obj.Index.ToString();
            this.Year = obj.Year.ToString();
            this.NamesViewModel.ReadFromObject(obj);
            this.EpisodesCount = obj.EpisodesCount.ToString();

            this.DayOfWeek = this.GetDayOfWeekValue(obj.DayOfWeek);
            this.EpisodeOffset = obj.EpisodeOffset ?? 0;

            var parent = this.Parent.ThrowIfNull().Source;
            var lastId = obj.LastVideoId;
            if (lastId != null)
            {
                var lastVideo = parent.Videos.FirstOrDefault(z => z.Id == lastId);
                if (lastVideo != null) this.LastVideoViewModel = new VideoInfoReadonlyViewModel(lastVideo);
            }
            var nextId = obj.NextVideoId;
            if (nextId != null)
            {
                var nextVideo = parent.Videos.FirstOrDefault(z => z.Id == nextId);
                if (nextVideo != null) this.NextVideoViewModel = new VideoInfoReadonlyViewModel(nextVideo);
            }

            this.TagsViewModel.ReadTags(obj);
        }

        public async Task LoadAsync()
        {
            // year
            this.YearCollection.AddRange(Enumerable.Range(DateTime.Now.Year - 7, 8).Select(z => z.ToString()));

            // index
            this.IndexCollection.AddRange(Enumerable.Range(1, 8).Select(z => z.ToString()));

            // episodes count
            this.EpisodesCountCollection.AddRange(new[] { 1, 8, 10, 11, 12, 13, 22, 24, 25, 26 }.Select(z => z.ToString()));

            // day of week
            this.DayOfWeekCollection.Add(new NameValuePair<string, DayOfWeek?>("None", null));
            this.DayOfWeekCollection.AddRange(
                ((DayOfWeek[])Enum.GetValues(typeof(DayOfWeek)))
                .Select(z => new NameValuePair<string, DayOfWeek?>(z.GetLocalizeString(), z)));

            // type
            var types = (await this.GetManagers().FlagManager.LoadAsync(JryFlagType.VideoType)).ToArray();
            this.TypeCollection.AddRange(types.Select(z => z.Value));
            if (this.Type.IsNullOrWhiteSpace())
                this.Type = this.TypeCollection.FirstOrDefault();

            // initialize cover
            if (this.Source?.CoverId != null)
            {
                var cover = await this.GetManagers().CoverManager.FindAsync(this.Source.CoverId);
                if (cover != null)
                {
                    this.ImageViewModel = ImageViewModel.Build(cover.BinaryData);
                }
            }
        }

        private static async Task<bool> IsInvalid<T>(MetroWindow window, object obj, string fieldName) where T : ValidationRule, new()
        {
            var error = new T().Validate(obj, CultureInfo.CurrentCulture);
            if (error.IsValid) return false;
            await window.ShowMessageAsync("error", $"error {fieldName} :\r\n {error.ErrorContent}");
            return true;
        }

        public async Task CommitAsync(MetroWindow window)
        {
            var parent = this.Parent.ThrowIfNull();

            if (this.Type.IsNullOrWhiteSpace())
            {
                await window.ShowMessageAsync("error", "must set a type.");
                return;
            }

            if (await IsInvalid<VideoYearValidationRule>(window, this.Year, "year") ||
                await IsInvalid<VideoIndexValidationRule>(window, this.Index, "index") ||
                await IsInvalid<VideoEpisodesCountValidationRule>(window, this.EpisodesCount, "episodes count"))
            {
                return;
            }

            var obj = this.GetCommitObject();

            if (this.Action == ObjectChangedAction.Create)
                obj.BuildMetaData();

            this.WriteToObject(obj);

            if (this.Cover != null)
            {
                var cover = await this.Cover.CommitAsync();
                obj.CoverId = cover.Id;
            }

            if (await base.CommitAsync(this.GetManagers().SeriesManager.GetVideoInfoManager(parent), obj) == null)
            {
                await window.ShowMessageAsync("error", "commit failed.");
            }
        }
    }
}