using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace JryVideo.Add.VideoCreator
{
    public class VideoCreatorViewModel : JasilyViewModel
    {
        private VideoInfoViewModel selected;

        public ObservableCollection<VideoInfoViewModel> VideoCollection { get; } = new ObservableCollection<VideoInfoViewModel>();

        public SeriesViewModel Source { get; }

        public VideoCreatorViewModel(SeriesViewModel source)
        {
            this.Source = source;
            this.VideoCollection.AddRange(source.VideoViewModels);
        }

        public VideoInfoViewModel Selected
        {
            get { return this.selected; }
            set { this.SetPropertyRef(ref this.selected, value); }
        }
    }
}