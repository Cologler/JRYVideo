using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class VideoViewModel : HasCoverViewModel<Model.JryVideo>
    {
        public VideoViewModel(JrySeries series, Model.JryVideo source)
            : base(source)
        {
            this.Series = series;
        }

        public JrySeries Series { get; private set; }

        public string SeriesName
        {
            get { return this.Series.Names.First(); }
        }

        public string Index
        {
            get { return String.Format("({0}) {1}", this.Source.Year, this.Source.Index); }
        }

        public string VideoName
        {
            get { return this.Source.Names.FirstOrDefault() ?? ""; }
        }

        protected override async Task<bool> TryAutoAddCoverAsync()
        {
            if (this.Source.DoubanId == null) return false;

            var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;

            var guid = await coverManager.GetCoverFromDoubanIdAsync(JryCoverType.Video, this.Source.DoubanId);

            if (guid == null) return false;

            this.Source.CoverId = guid;

            var seriesManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager;
            await seriesManager.UpdateAsync(this.Series);
            return true;
        }

        public static IEnumerable<VideoViewModel> Create(JrySeries series)
        {
            return series.Videos.Select(jryVideo => new VideoViewModel(series, jryVideo));
        }
    }
}