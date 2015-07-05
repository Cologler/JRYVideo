using System;
using System.ComponentModel;
using System.Linq;
using JryVideo.Model;

namespace JryVideo.Common
{
    public sealed class SeriesViewModel : JasilyViewModel<JrySeries>
    {
        private string displayName;
        private string displayNameFirstLine;
        private string displayNameInfo;
        private string displayNameOtherLines;
        private bool hasOtherLine;
        private string displayFullName;

        public SeriesViewModel(JrySeries source)
            : base(source)
        {
            this.Reload();
        }

        /// <summary>
        /// like A / B / C
        /// </summary>
        public string DisplayName
        {
            get { return this.displayName; }
            private set { this.SetPropertyRef(ref this.displayName, value); }
        }

        /// <summary>
        /// like A
        /// </summary>
        public string DisplayNameFirstLine
        {
            get { return this.displayNameFirstLine; }
            private set { this.SetPropertyRef(ref this.displayNameFirstLine, value); }
        }

        public bool HasOtherLine
        {
            get { return this.hasOtherLine; }
            private set { this.SetPropertyRef(ref this.hasOtherLine, value); }
        }

        /// <summary>
        /// like B\r\nC ( max count 3)
        /// </summary>
        public string DisplayNameOtherLines
        {
            get { return this.displayNameOtherLines; }
            private set { this.SetPropertyRef(ref this.displayNameOtherLines, value); }
        }

        /// <summary>
        /// like A \r\n B \r\n C
        /// </summary>
        public string DisplayFullName
        {
            get { return this.displayFullName; }
            private set { this.SetPropertyRef(ref this.displayFullName, value); }
        }

        /// <summary>
        /// like ({0} videos) this.DisplayName
        /// </summary>
        public string DisplayNameInfo
        {
            get { return this.displayNameInfo; }
            private set { this.SetPropertyRef(ref this.displayNameInfo, value); }
        }

        public void Reload()
        {
            this.DisplayName = String.Join(" / ", this.Source.Names);
            this.DisplayNameInfo = String.Format("({0} videos) {1}", this.Source.Videos.Count, this.DisplayName);
            this.DisplayNameFirstLine = this.Source.Names[0];
            this.HasOtherLine = this.Source.Names.Count > 1;
            this.DisplayNameOtherLines = this.HasOtherLine ? this.Source.Names.Skip(1).Take(3).AsLines() : "";
            this.DisplayFullName = this.Source.Names.AsLines();
        }
    }
}