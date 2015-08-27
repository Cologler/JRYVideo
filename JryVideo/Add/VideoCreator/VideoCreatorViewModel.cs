using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Model;

namespace JryVideo.Add.VideoCreator
{
    public class VideoCreatorViewModel : JasilyViewModel<JrySeries>
    {
        private VideoInfoViewModel selected;

        public ObservableCollection<VideoInfoViewModel> VideoCollection { get; private set; }

        public async Task LoadAsync()
        {
            var series = this.Source;

            this.VideoCollection.AddRange(
                await Task.Run(() => VideoInfoViewModel.Create(series)));
        }

        public VideoCreatorViewModel(JrySeries source)
            : base(source)
        {
            this.VideoCollection = new ObservableCollection<VideoInfoViewModel>();
        }

        public VideoInfoViewModel Selected
        {
            get { return this.selected; }
            set { this.SetPropertyRef(ref this.selected, value); }
        }
    }
}