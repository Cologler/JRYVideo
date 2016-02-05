using Jasily.ComponentModel;
using JryVideo.Model;
using System;
using System.Linq;

namespace JryVideo.Common
{
    public class VideoInfoReadonlyViewModel : HasCoverViewModel<JryVideoInfo>
    {
        public VideoInfoReadonlyViewModel(JryVideoInfo source)
            : base(source)
        {
            this.NameViewModel = new NameableViewModel<JryVideoInfo>(source);
        }

        public NameableViewModel<JryVideoInfo> NameViewModel { get; }

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

        public static implicit operator JryVideoInfo(VideoInfoReadonlyViewModel viewModel) => viewModel?.Source;

        public static implicit operator VideoInfoReadonlyViewModel(JryVideoInfo video) =>
            video == null ? null : new VideoInfoReadonlyViewModel(video);
    }
}