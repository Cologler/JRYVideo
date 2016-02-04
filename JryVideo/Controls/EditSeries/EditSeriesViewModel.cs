using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
using JryVideo.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using static JryVideo.Common.Helper;

namespace JryVideo.Controls.EditSeries
{
    public class EditSeriesViewModel : EditorItemViewModel<JrySeries>
    {
        private string names;
        private string doubanId;
        private string imdbId;
        private string theTVDBId;

        public string Names
        {
            get { return this.names; }
            set { this.SetPropertyRef(ref this.names, value); }
        }

        public override void ReadFromObject(JrySeries obj)
        {
            base.ReadFromObject(obj);

            this.Names = this.Source == null ? "" : this.Source.Names.AsLines();
        }

        public override void WriteToObject(JrySeries obj)
        {
            base.WriteToObject(obj);

            obj.Names.Clear();

            if (!String.IsNullOrWhiteSpace(this.Names))
            {
                obj.Names.AddRange(
                    this.Names.AsLines()
                        .Select(z => z.Trim())
                        .Where(z => !String.IsNullOrWhiteSpace(z)));
                obj.Names = obj.Names.Distinct().ToList();
            }

            obj.ImdbId = obj.ImdbId.IsNullOrWhiteSpace() ? null : obj.ImdbId.Trim();
            obj.TheTVDBId = obj.TheTVDBId.IsNullOrWhiteSpace() ? null : obj.TheTVDBId.Trim();
        }

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
            if (String.IsNullOrWhiteSpace(this.DoubanId)) return;

            var movie = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);

            if (movie != null)
            {
                var doubanName = DoubanMovieParser.Parse(movie).SeriesNames.AsLines();

                this.Names = String.IsNullOrWhiteSpace(this.Names)
                    ? doubanName
                    : String.Join("\r\n", this.Names, doubanName);
            }
        }

        public async Task<JrySeries> CommitAsync()
        {
            var series = this.GetCommitObject().ThrowIfNull("series");

            this.WriteToObject(series);

            SeriesManager.BuildSeriesMetaData(series);

            var seriesManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager;

            return await this.CommitAsync(seriesManager, series);
        }

        public override void Clear()
        {
            this.Names = "";
            this.DoubanId = "";
        }
    }
}