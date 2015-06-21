using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class VideoViewModel : JasilyViewModel<Model.JryVideo>
    {
        private JryCover _cover;

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

        public JryCover Cover
        {
            get
            {
                var cover = this._cover;

                if (cover == null)
                {
                    this.BeginUpdateCover();
                    return null;
                }
                else
                {
                    this._cover = null; // clear memery.
                    return cover;
                }
            }
            set
            {
                this.SetPropertyRef(ref this._cover, value);
            }
        }

        public async Task<JryCover> TryGetCoverAsync()
        {
            if (this.Source.CoverId == Guid.Empty) return null;

            return await JryVideoCore.Current.CurrentDataCenter.CoverManager.LoadCoverAsync(this.Source.CoverId);
        }

        public async void BeginUpdateCover()
        {
            var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;
            
            if (this.Source.CoverId == Guid.Empty)
            {
                if (this.Source.DoubanId == null) return;

                var guid = await coverManager.UpdateCoverFromDoubanIdAsync(this.Source.DoubanId);

                if (guid == null) return;

                this.Source.CoverId = guid.Value;

                var seriesManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager;
                await seriesManager.UpdateAsync(this.Series);
            }

            this.Cover = await JryVideoCore.Current.CurrentDataCenter.CoverManager.LoadCoverAsync(this.Source.CoverId);
        }

        public static IEnumerable<VideoViewModel> Create(JrySeries series)
        {
            return series.Videos.Select(jryVideo => new VideoViewModel(series, jryVideo));
        }
    }
}