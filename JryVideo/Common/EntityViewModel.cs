using System;
using System.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Common
{
    public sealed class EntityViewModel : JasilyViewModel<JryEntity>
    {
        private string displayExtension;
        private string displayFansubs;
        private string displaySubTitleLanguages;
        private string displayTrackLanguages;
        private string displayFormat;

        public EntityViewModel(JryEntity source)
            : base(source)
        {
            this.Reload();
        }

        public string DisplayExtension
        {
            get { return this.displayExtension; }
            private set { this.SetPropertyRef(ref this.displayExtension, value); }
        }

        public string DisplayFansubs
        {
            get { return this.displayFansubs; }
            private set { this.SetPropertyRef(ref this.displayFansubs, value); }
        }

        public string DisplaySubTitleLanguages
        {
            get { return this.displaySubTitleLanguages; }
            private set { this.SetPropertyRef(ref this.displaySubTitleLanguages, value); }
        }

        public string DisplayTrackLanguages
        {
            get { return this.displayTrackLanguages; }
            private set { this.SetPropertyRef(ref this.displayTrackLanguages, value); }
        }

        public string DisplayFormat
        {
            get { return this.displayFormat; }
            private set { this.SetPropertyRef(ref this.displayFormat, value); }
        }

        public void Reload()
        {
            this.DisplayExtension = this.Source.Extension.ToUpper();
            this.DisplayFansubs = this.Source.Fansubs.AsLines(" / ");
            this.DisplaySubTitleLanguages = this.Source.SubTitleLanguages.AsLines(" / ");
            this.DisplayTrackLanguages = this.Source.TrackLanguages.AsLines(" / ");
            this.DisplayFormat =
                this.Source.Format == null
                ? ""
                : String.Format("({0}), {1}", this.Source.Format.Type, this.Source.Format.Value);
        }
    }
}