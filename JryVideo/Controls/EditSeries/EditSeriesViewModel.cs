using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Controls.SelectFlag;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
using JryVideo.Model;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static JryVideo.Common.Helper;

namespace JryVideo.Controls.EditSeries
{
    public class EditSeriesViewModel : EditorItemViewModel<JrySeries>
    {
        private string doubanId;
        private string imdbId;
        private string theTVDBId;

        public NameEditableViewModel<JrySeries> NamesViewModel { get; }
            = new NameEditableViewModel<JrySeries>(false);

        public override void ReadFromObject(JrySeries obj)
        {
            base.ReadFromObject(obj);

            this.NamesViewModel.ReadFromObject(obj);
            this.TagsViewModel.ReadTags(obj);
        }

        public override void WriteToObject(JrySeries obj)
        {
            base.WriteToObject(obj);

            this.NamesViewModel.WriteToObject(obj);
            obj.ImdbId = obj.ImdbId.IsNullOrWhiteSpace() ? null : obj.ImdbId.Trim();
            obj.TheTVDBId = obj.TheTVDBId.IsNullOrWhiteSpace() ? null : obj.TheTVDBId.Trim();
            this.TagsViewModel.WriteTags(obj, true);
        }

        public SelectFlagViewModel TagsViewModel { get; } = new SelectFlagViewModel(JryFlagType.SeriesTag);

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
        public string TheTVDBId
        {
            get { return this.theTVDBId; }
            set { this.SetPropertyRef(ref this.theTVDBId, value); }
        }

        public async Task LoadDoubanAsync()
        {
            if (string.IsNullOrWhiteSpace(this.DoubanId)) return;

            var movie = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);

            if (movie != null)
            {
                this.NamesViewModel.AddRange(DoubanMovieParser.Parse(movie).SeriesNames);
            }
        }

        public async Task<JrySeries> CommitAsync()
        {
            var series = this.GetCommitObject();
            Debug.Assert(series != null);

            this.WriteToObject(series);

            SeriesManager.BuildSeriesMetaData(series);

            return await this.CommitAsync(this.GetManagers().SeriesManager, series);
        }

        public override void Clear()
        {
            this.NamesViewModel.Names = "";
            this.DoubanId = "";
        }
    }
}