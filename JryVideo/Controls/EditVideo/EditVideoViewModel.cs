using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JryVideo.Common;
using JryVideo.Common.ValidationRules;
using JryVideo.Core;
using JryVideo.Core.Douban;
using JryVideo.Editors.CoverEditor;
using JryVideo.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace JryVideo.Controls.EditVideo
{
    public class EditVideoViewModel : EditorItemViewModel<Model.JryVideoInfo>
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

        public EditVideoViewModel()
        {
            this.TypeCollection = new ObservableCollection<string>();
            this.YearCollection = new ObservableCollection<string>();
            this.IndexCollection = new ObservableCollection<string>();
            this.EpisodesCountCollection = new ObservableCollection<string>();
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

        public string SelectedVideoType
        {
            get { return this.selectedType; }
            set { this.SetPropertyRef(ref this.selectedType, value); }
        }

        public string Year
        {
            get { return this.year; }
            set { this.SetPropertyRef(ref this.year, value); }
        }

        public string DoubanId
        {
            get { return this.doubanId; }
            set { this.SetPropertyRef(ref this.doubanId, value); }
        }

        public string ImdbId
        {
            get { return this.imdbId; }
            set { this.SetPropertyRef(ref this.imdbId, value); }
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
            if (String.IsNullOrWhiteSpace(this.DoubanId)) return;

            var info = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);

            if (info != null)
            {
                this.LoadDouban(info);
            }
        }

        public void LoadDouban(DoubanMovie info)
        {
            var doubanSecondName = DoubanHelper.ParseSecondName(info).AsLines();

            this.Names = String.IsNullOrWhiteSpace(this.Names)
                ? doubanSecondName
                : String.Join("\r\n", this.Names, doubanSecondName);

            if (String.IsNullOrWhiteSpace(this.Year))
            {
                this.Year = info.Year.ToString();
            }

            if (String.IsNullOrWhiteSpace(this.Index))
            {
                var doubanMainNames = DoubanHelper.ParseMainName(info);
                var group = doubanMainNames
                    .Select(z => IndexParse.Match(z))
                    .Where(z => z.Success)
                    .Select(z => z.Groups[1])
                    .FirstOrDefault();
                if (group != null) this.Index = group.Value;
            }
        }

        private static readonly Regex IndexParse = new Regex("(\\d)+$");

        public async Task LoadAsync()
        {
            // type
            var types = (await JryVideoCore.Current.CurrentDataCenter.FlagManager.LoadAsync(JryFlagType.VideoType)).ToArray();
            this.TypeCollection.AddRange(types.Select(z => z.Value));
            this.SelectedVideoType = this.TypeCollection.FirstOrDefault();

            // year
            this.YearCollection.AddRange(Enumerable.Range(DateTime.Now.Year - 7, 8).Select(z => z.ToString()));

            // index
            this.IndexCollection.AddRange(Enumerable.Range(1, 8).Select(z => z.ToString()));

            // episodes count
            this.EpisodesCountCollection.AddRange(Enumerable.Range(1, 8).Select(z => z.ToString()));

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

        public async Task<bool> CommitAsync(MetroWindow window)
        {
            var parent = this.Parent.ThrowIfNull("Parent");

            if (String.IsNullOrWhiteSpace(this.SelectedVideoType))
            {
                await window.ShowMessageAsync("error", "must set a type.");
                return false;
            }

            var error = new VideoYearValidationRule().Validate(this.Year, CultureInfo.CurrentCulture);
            if (!error.IsValid)
            {
                await window.ShowMessageAsync("error", String.Format("error {0} :\r\n {1}", "year", error.ErrorContent.ToString()));
                return false;
            }

            error = new VideoIndexValidationRule().Validate(this.Index, CultureInfo.CurrentCulture);
            if (!error.IsValid)
            {
                await window.ShowMessageAsync("error", String.Format("error {0} :\r\n {1}", "index", error.ErrorContent.ToString()));
                return false;
            }

            error = new VideoEpisodesCountValidationRule().Validate(this.EpisodesCount, CultureInfo.CurrentCulture);
            if (!error.IsValid)
            {
                await window.ShowMessageAsync("error", String.Format("error {0} :\r\n {1}", "episodes count", error.ErrorContent.ToString()));
                return false;
            }

            var obj = this.GetCommitObject();

            obj.Type = this.selectedType;
            obj.Year = Int32.Parse(this.Year);
            obj.Index = Int32.Parse(this.Index);
            obj.DoubanId = this.DoubanId;
            obj.ImdbId = this.ImdbId;
            obj.EpisodesCount = Int32.Parse(this.EpisodesCount);
            obj.Names.Clear();
            obj.Names.AddRange(
                (this.Names.AsLines() ?? Enumerable.Empty<String>())
                    .Select(z => z.Trim())
                    .Where(z => !String.IsNullOrWhiteSpace(z)));
            obj.Names = obj.Names.Distinct().ToList();
            obj.BuildMetaData();

            var videoManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(parent);

            return await base.CommitAsync(videoManager, obj);
        }
    }
}