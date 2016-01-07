using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Common.ValidationRules;
using JryVideo.Core;
using JryVideo.Core.Douban;
using JryVideo.Editors.CoverEditor;
using JryVideo.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Enums;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
        private string names;
        private string episodesCount;
        private bool isTracking;
        private NameValuePair<string, DayOfWeek?>? dayOfWeek;
        private DateTime? startDate;

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
            set { this.SetPropertyRef(ref this.doubanId, value); }
        }

        [EditableField]
        public string ImdbId
        {
            get { return this.imdbId; }
            set
            {
                if (value != null && !value.StartsWith("tt"))
                {
                    var index = value.IndexOf("tt", StringComparison.Ordinal);
                    if (index > -1) value = value.Substring(index);
                }
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
        public DateTime? StartLocalDate
        {
            get { return this.startDate; }
            set
            {
                if (value.HasValue)
                {
                    if (this.DayOfWeek == null)
                    {
                        this.DayOfWeek = this.GetDayOfWeekValue(value.Value.DayOfWeek);
                    }
                    var today = DateTime.Today;
                    if (value.Value >= today.AddDays(-(int)today.DayOfWeek)) // this week
                    {
                        this.IsTracking = true;
                    }
                }
                this.SetPropertyRef(ref this.startDate, value);
            }
        }

        public NameValuePair<string, DayOfWeek?>? DayOfWeek
        {
            get { return this.dayOfWeek; }
            set { this.SetPropertyRef(ref this.dayOfWeek, value); }
        }

        public JrySeries Parent { get; set; }

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

        public string Names
        {
            get { return this.names; }
            set { this.SetPropertyRef(ref this.names, value); }
        }

        public string EpisodesCount
        {
            get { return this.episodesCount; }
            set { this.SetPropertyRef(ref this.episodesCount, value); }
        }

        public async Task LoadDoubanAsync()
        {
            if (this.DoubanId.IsNullOrWhiteSpace()) return;

            var info = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);

            if (info != null)
            {
                this.LoadDouban(info);
            }
        }

        private NameValuePair<string, DayOfWeek?> GetDayOfWeekValue(DayOfWeek? dow)
            => this.DayOfWeekCollection.First(z => z.Value == dow);

        public override void WriteToObject(JryVideoInfo obj)
        {
            base.WriteToObject(obj);

            obj.Year = Int32.Parse(this.Year);
            obj.Index = Int32.Parse(this.Index);
            obj.EpisodesCount = Int32.Parse(this.EpisodesCount);
            obj.Names.Clear();
            if (!this.Names.IsNullOrWhiteSpace())
            {
                obj.Names.AddRange(this.Names.AsLines().Select(z => z.Trim()).Where(z => !z.IsNullOrWhiteSpace()).Distinct());
            }
            obj.DayOfWeek = this.DayOfWeek?.Value;
        }

        public override void ReadFromObject(JryVideoInfo obj)
        {
            base.ReadFromObject(obj);

            this.Index = obj.Index.ToString();
            this.Year = obj.Year.ToString();
            this.Names = obj.Names.AsLines();
            this.EpisodesCount = obj.EpisodesCount.ToString();

            this.DayOfWeek = this.GetDayOfWeekValue(obj.DayOfWeek);
        }

        public void LoadDouban(DoubanMovie info)
        {
            var parser = DoubanMovieParser.Parse(info);
            var doubanSecondName = parser.EntityNames.AsLines();

            this.Names = this.Names.IsNullOrWhiteSpace()
                ? doubanSecondName
                : String.Join("\r\n", this.Names, doubanSecondName);

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
            var types = (await JryVideoCore.Current.CurrentDataCenter.FlagManager.LoadAsync(JryFlagType.VideoType)).ToArray();
            this.TypeCollection.AddRange(types.Select(z => z.Value));
            if (this.Type.IsNullOrWhiteSpace())
                this.Type = this.TypeCollection.FirstOrDefault();

            // initialize cover
            if (this.Source != null && this.Source.CoverId != null)
            {
                var cover = await JryVideoCore.Current.CurrentDataCenter.CoverManager.FindAsync(this.Source.CoverId);
                if (cover != null)
                {
                    this.ImageViewModel = ImageViewModel.Build(cover.BinaryData);
                }
            }
        }

        public async Task<JryVideoInfo> CommitAsync(MetroWindow window)
        {
            var parent = this.Parent.ThrowIfNull("Parent");

            if (this.Type.IsNullOrWhiteSpace())
            {
                await window.ShowMessageAsync("error", "must set a type.");
                return null;
            }

            var error = new VideoYearValidationRule().Validate(this.Year, CultureInfo.CurrentCulture);
            if (!error.IsValid)
            {
                await window.ShowMessageAsync("error", String.Format("error {0} :\r\n {1}", "year", error.ErrorContent.ToString()));
                return null;
            }

            error = new VideoIndexValidationRule().Validate(this.Index, CultureInfo.CurrentCulture);
            if (!error.IsValid)
            {
                await window.ShowMessageAsync("error", String.Format("error {0} :\r\n {1}", "index", error.ErrorContent.ToString()));
                return null;
            }

            error = new VideoEpisodesCountValidationRule().Validate(this.EpisodesCount, CultureInfo.CurrentCulture);
            if (!error.IsValid)
            {
                await window.ShowMessageAsync("error", String.Format("error {0} :\r\n {1}", "episodes count", error.ErrorContent.ToString()));
                return null;
            }

            var obj = this.GetCommitObject();

            this.WriteToObject(obj);

            if (this.Action == ObjectChangedAction.Create)
                obj.BuildMetaData();

            if (this.Cover?.Action == ObjectChangedAction.Create)
            {
                var cover = await this.Cover.CommitAsync();
                obj.CoverId = cover.Id;
            }

            var videoManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(parent);

            return await base.CommitAsync(videoManager, obj);
        }
    }
}