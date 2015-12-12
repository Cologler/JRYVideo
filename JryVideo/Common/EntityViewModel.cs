using Jasily.ComponentModel;
using JryVideo.Model;
using System;
using System.Collections.Generic;

namespace JryVideo.Common
{
    public sealed class EntityViewModel : JasilyViewModel<JryEntity>
    {
        public EntityViewModel(JryEntity source)
            : base(source)
        {
            this.RefreshProperties();
        }

        [NotifyPropertyChanged]
        public string DisplayExtension => this.Source.Extension.ToUpper();

        [NotifyPropertyChanged]
        public string DisplayFansubs => ListToLine(this.Source.Fansubs);

        [NotifyPropertyChanged]
        public string DisplaySubTitleLanguages => ListToLine(this.Source.SubTitleLanguages);

        [NotifyPropertyChanged]
        public bool HasSubTitleLanguages => this.Source.SubTitleLanguages.Count > 0;

        [NotifyPropertyChanged]
        public bool DontHasSubTitleLanguages => this.Source.SubTitleLanguages.Count == 0;

        [NotifyPropertyChanged]
        public string DisplayTrackLanguages => ListToLine(this.Source.TrackLanguages);

        [NotifyPropertyChanged]
        public string DisplayTags => ListToLine(this.Source.Tags);

        [NotifyPropertyChanged]
        public string DisplayFormat => this.Source.Format == null
            ? null
            : string.Format("({0}), {1}", this.Source.Format.Type, this.Source.Format.Value);

        private static string ListToLine(List<string> lines) => lines.Count > 0 ? lines.AsLines(" / ") : "[EMPTY]";
    }
}