using System;
using System.ComponentModel;
using System.Linq;
using JryVideo.Model;

namespace JryVideo.Common
{
    public sealed class EntityViewModel : JasilyViewModel<JryEntity>
    {
        public EntityViewModel(JryEntity source)
            : base(source)
        {
        }

        public string DisplayExtension
        {
            get { return this.Source.Extension.ToUpper(); }
        }

        public string DisplayFansubs
        {
            get { return this.Source.Fansubs.AsLines(" / "); }
        }

        public string DisplaySubTitleLanguages
        {
            get { return this.Source.SubTitleLanguages.AsLines(" / "); }
        }

        public string DisplayTrackLanguages
        {
            get { return this.Source.TrackLanguages.AsLines(" / "); }
        }

        public string DisplayFormat
        {
            get
            {
                return this.Source.Format == null
                    ? ""
                    : String.Format("({0}), {1}",
                        this.Source.Format.Type,
                        this.Source.Format.Value);
            }
        }
    }
}