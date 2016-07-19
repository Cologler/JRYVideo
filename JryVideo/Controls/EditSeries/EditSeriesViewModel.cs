using System;
using System.Diagnostics;
using System.Enums;
using System.Threading.Tasks;
using Jasily.ComponentModel;
using Jasily.ComponentModel.Editable;
using JryVideo.Common;
using JryVideo.Controls.SelectFlag;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
using JryVideo.Core.Managers.Upgrades;
using JryVideo.Model;
using static JryVideo.Common.Helper;

namespace JryVideo.Controls.EditSeries
{
    public class EditSeriesViewModel : EditorItemViewModel<Series>
    {
        [EditableField(IsSubEditableViewModel = true)]
        public NameEditableViewModel<Series> NamesViewModel { get; }
            = new NameEditableViewModel<Series>(false);

        public override void ReadFromObject(Series obj)
        {
            base.ReadFromObject(obj);
            this.TagsViewModel.ReadTags(obj);
        }

        public override void WriteToObject(Series obj)
        {
            base.WriteToObject(obj);
            obj.ImdbId = obj.ImdbId.IsNullOrWhiteSpace() ? null : obj.ImdbId.Trim();
            obj.TheTVDBId = obj.TheTVDBId.IsNullOrWhiteSpace() ? null : obj.TheTVDBId.Trim();
            this.TagsViewModel.WriteTags(obj, true);
        }

        public SelectFlagViewModel TagsViewModel { get; } = new SelectFlagViewModel(JryFlagType.SeriesTag);

        public Property<string> DoubanId { get; } = new Property<string> { SetterConverter = TryGetDoubanId };

        [EditableField]
        public Property<string> ImdbId { get; } = new Property<string> { SetterConverter = TryGetImdbId };

        [EditableField]
        public Property<string> TheTVDBId { get; } = new Property<string>();

        public async Task LoadDoubanAsync()
        {
            if (string.IsNullOrWhiteSpace(this.DoubanId)) return;

            var movie = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);

            if (movie != null)
            {
                this.NamesViewModel.AddRange(DoubanMovieParser.Parse(movie).SeriesNames);
            }
        }

        public async Task<Series> CommitAsync()
        {
            var series = this.GetCommitObject();
            Debug.Assert(series != null);
            if (this.Action == ObjectChangedAction.Create) series.Version = Upgrader<Series>.MaxVersion;
            this.WriteToObject(series);

            SeriesManager.BuildSeriesMetaData(series);

            return await this.CommitAsync(this.GetManagers().SeriesManager, series);
        }

        protected override void Clear()
        {
            this.NamesViewModel.Names = "";
            this.DoubanId.Value = "";
        }
    }
}