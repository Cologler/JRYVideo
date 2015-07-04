using System;
using System.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Selectors.SeriesSelector
{
    public sealed class SeriesViewModel : JasilyViewModel<JrySeries>
    {
        private string displayName;

        public SeriesViewModel(JrySeries source)
            : base(source)
        {
            this.Reload();
        }

        public string DisplayName
        {
            get { return this.displayName; }
            set { this.SetPropertyRef(ref this.displayName, value); }
        }

        public void Reload()
        {
            this.DisplayName = String.Format("({0} videos) {1}", this.Source.Videos.Count, String.Join(" / ", this.Source.Names));
        }
    }
}