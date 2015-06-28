using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Enums;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Controls.EditSeries
{
    public class EditSeriesViewModel : EditorItemViewModel<JrySeries>
    {
        private string names;
        private string doubanId;

        public EditSeriesViewModel()
        {
            this.UpdateNames();
        }

        public string Names
        {
            get { return this.names; }
            set { this.SetPropertyRef(ref this.names, value); }
        }

        public void UpdateNames()
        {
            this.Names = this.Source == null ? "" : this.Source.Names.AsLines();
        }

        public string DoubanId
        {
            get { return this.doubanId; }
            set { this.SetPropertyRef(ref this.doubanId, value); }
        }

        public DoubanMovie DoubanMovie { get; private set; }

        public async Task LoadDoubanAsync()
        {
            if (String.IsNullOrWhiteSpace(this.DoubanId)) return;

            this.DoubanMovie = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);

            if (this.DoubanMovie != null)
            {
                var doubanName = DoubanHelper.ParseMainName(this.DoubanMovie).AsLines();

                this.Names = String.IsNullOrWhiteSpace(this.Names)
                    ? doubanName
                    : String.Join("\r\n", this.Names, doubanName);
            }
        }

        public async Task<bool> CommitAsync()
        {
            var series = this.GetCommitObject().ThrowIfNull("series");

            series.Names.Clear();
            series.Names.AddRange(
                this.Names.AsLines()
                    .Select(z => z.Trim())
                    .Where(z => !String.IsNullOrWhiteSpace(z)));
            series.Names = series.Names.Distinct().ToList();

            SeriesManager.BuildSeriesMetaData(series);

            var seriesManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager;
            
            return await this.CommitAsync(seriesManager, series);
        }
    }
}