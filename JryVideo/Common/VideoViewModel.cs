using System;
using System.ComponentModel;
using System.Linq;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class VideoViewModel : JasilyViewModel<Model.JryVideo>
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

        public byte[] Cover
        {
            get { return new CoverManager() [this.Source.CoverId]; }
        }
    }
}