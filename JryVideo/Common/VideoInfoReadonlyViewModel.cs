using System;
using System.Linq;
using Jasily.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class VideoInfoReadonlyViewModel : JasilyViewModel<JryVideoInfo>
    {
        public VideoInfoReadonlyViewModel(JryVideoInfo source)
            : base(source)
        {
            this.NameViewModel = new NameableViewModel<JryVideoInfo>(source);
            this.CoverViewModel = new CoverViewModel(this.Source);
        }

        public NameableViewModel<JryVideoInfo> NameViewModel { get; }

        public CoverViewModel CoverViewModel { get; }

        [NotifyPropertyChanged]
        public string YearWithIndex => $"({this.Source.Year}) {this.Source.Index}";

        [NotifyPropertyChanged]
        public string VideoNames => this.Source.Names.FirstOrDefault() ?? String.Empty;

        [NotifyPropertyChanged]
        public string VideoFullNames => this.Source.Names.Count == 0 ? null : this.Source.Names.AsLines();

        [NotifyPropertyChanged]
        public bool IsNotAllAired => !this.Source.IsAllAired;

        [NotifyPropertyChanged]
        public bool HasLast => this.Source.LastVideoId != null;

        [NotifyPropertyChanged]
        public bool HasNext => this.Source.NextVideoId != null;

        /// <summary>
        /// the method will call PropertyChanged for each property which has [NotifyPropertyChanged]
        /// </summary>
        public override void RefreshProperties()
        {
            base.RefreshProperties();
            this.NameViewModel.RefreshProperties();
        }
    }
}