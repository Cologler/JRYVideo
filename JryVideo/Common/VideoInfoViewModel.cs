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
    public class VideoInfoViewModel : HasCoverViewModel<Model.JryVideoInfo>
    {
        public VideoInfoViewModel(JrySeries series, Model.JryVideoInfo source)
            : base(source)
        {
            this.SeriesView = new SeriesViewModel(series);
        }

        public SeriesViewModel SeriesView { get; private set; }

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

            var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(this.SeriesView.Source);
            return await manager.UpdateAsync(this.Source);
        }

        public static IEnumerable<VideoInfoViewModel> Create(JrySeries series)
        {
            return series.Videos.Select(jryVideo => new VideoInfoViewModel(series, jryVideo));
        }
    }
}