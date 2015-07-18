using System;
using System.ComponentModel;
using System.Linq;
using JryVideo.Model;

namespace JryVideo.Common
{
    public sealed class SeriesViewModel : JasilyViewModel<JrySeries>
    {
        public SeriesViewModel(JrySeries source)
            : base(source)
        {
        }

        /// <summary>
        /// like A
        /// </summary>
        [NotifyPropertyChanged]
        public string DisplayNameFirstLine
        {
            get { return this.Source.Names[0]; }
        }

        /// <summary>
        /// like B\r\nC ( max count 3)
        /// </summary>
        [NotifyPropertyChanged]
        public string DisplayNameSecondLine
        {
            get { return this.Source.Names.Count > 1 ? this.Source.Names[1] : null; }
        }

        /// <summary>
        /// like A \r\n B \r\n C
        /// </summary>
        [NotifyPropertyChanged]
        public string DisplayFullName
        {
            get { return this.Source.Names.AsLines(); }
        }

        /// <summary>
        /// like ({0} videos) this.DisplayName
        /// </summary>
        [NotifyPropertyChanged]
        public string DisplayNameInfo
        {
            get { return String.Format("({0} videos) {1}", this.Source.Videos.Count, String.Join(" / ", this.Source.Names)); }
        }
    }
}