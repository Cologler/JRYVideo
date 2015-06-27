using System;
using System.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Add.SelectSeries
{
    public sealed class SeriesViewModel : JasilyViewModel<JrySeries>
    {
        public SeriesViewModel(JrySeries source)
            : base(source)
        {
        }

        public string DisplayName
        {
            get { return String.Format("({0} videos) {1}", this.Source.Videos.Count, String.Join(" / ", this.Source.Names)); }
        }
    }
}