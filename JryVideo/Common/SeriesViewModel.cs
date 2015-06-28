using System;
using System.ComponentModel;
using System.Linq;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class SeriesViewModel : JasilyViewModel<JrySeries>
    {
        public SeriesViewModel(JrySeries source)
            : base(source)
        {
        }

        public string SeriesNameFirstLine
        {
            get { return this.Source.Names[0]; }
        }

        public string SeriesName
        {
            get { return String.Join(" / ", this.Source.Names.First()); }
        }
    }
}