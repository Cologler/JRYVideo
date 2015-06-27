using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using JryVideo.Common;
using JryVideo.Model;

namespace JryVideo.Add.VideoCreator
{
    public class VideoCreatorViewModel : JasilyViewModel<JrySeries>
    {
        private VideoViewModel selected;

        public ObservableCollection<VideoViewModel> VideoCollection { get; private set; }

        public async Task LoadAsync()
        {
            var series = this.Source;

            this.VideoCollection.AddRange(
                await Task.Run(() => VideoViewModel.Create(series)));
        }

        public VideoCreatorViewModel(JrySeries source)
            : base(source)
        {
            this.VideoCollection = new ObservableCollection<VideoViewModel>();
        }

        public VideoViewModel Selected
        {
            get { return this.selected; }
            set { this.SetPropertyRef(ref this.selected, value); }
        }
    }
}