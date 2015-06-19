using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class VideoViewModel : JasilyViewModel<Model.JryVideo>
    {
        private byte[] _cover;

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

        public byte[] Cover
        {
            get
            {
                if (this._cover == null)
                    this.BeginUpdateCover();

                return this._cover;
            }
            set
            {
                if (value != null)
                {
                    this.SetPropertyRef(ref this._cover, value);
                }
            }
        }

        private async void BeginUpdateCover()
        {
            var coverManager = JryVideoCore.Current.CoverManager;
            
            if (this.Source.CoverId == Guid.Empty)
            {
                if (this.Source.DoubanId == null) return;

                var guid = await coverManager.UpdateCoverFromDoubanIdAsync(this.Source.DoubanId);

                if (guid == null) return;

                this.Source.CoverId = guid.Value;

                var seriesManager = JryVideoCore.Current.SeriesManager;
                await seriesManager.UpdateAsync(this.Series);
            }

            var buff = await JryVideoCore.Current.CoverManager.LoadCoverAsync(this.Source.CoverId);

            this.Cover = buff;
        }

        public static IEnumerable<VideoViewModel> Create(JrySeries series)
        {
            return series.Videos.Select(jryVideo => new VideoViewModel(series, jryVideo));
        }
    }
}