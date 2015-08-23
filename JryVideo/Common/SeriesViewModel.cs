using System;
using System.ComponentModel;
using System.Linq;
using Jasily.Diagnostics;
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
        public string DisplayNameFirstLine => this.Source.Names[0];

        /// <summary>
        /// like B\r\nC ( max count 3)
        /// </summary>
        [NotifyPropertyChanged]
        public string DisplayNameSecondLine => this.Source.Names.Count > 1 ? this.Source.Names[1] : null;

        /// <summary>
        /// like A \r\n B \r\n C
        /// </summary>
        [NotifyPropertyChanged]
        public string DisplayFullName => this.Source.Names.AsLines();

        /// <summary>
        /// like ({0} videos) this.DisplayName
        /// </summary>
        [NotifyPropertyChanged]
        public string DisplayNameInfo => $"({this.Source.Videos.Count.ToString()} videos) {string.Join(" / ", this.Source.Names)}";
    }
}