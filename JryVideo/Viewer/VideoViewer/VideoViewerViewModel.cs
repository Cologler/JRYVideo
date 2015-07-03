using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Common;
using JryVideo.Core;

namespace JryVideo.Viewer.VideoViewer
{
    public sealed class VideoViewerViewModel : JasilyViewModel
    {
        private VideoViewModel video;

        public VideoViewerViewModel(VideoInfoViewModel info)
        {
            this.Info = info;
            this.EntitesView = new JasilyCollectionView<ObservableCollectionGroup<string, EntityViewModel>>();
        }

        public VideoInfoViewModel Info { get; private set; }

        public VideoViewModel Video
        {
            get { return this.video; }
            private set { this.SetPropertyRef(ref this.video, value); }
        }

        public async Task LoadAsync()
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.VideoManager;

            var video = await manager.FindAsync(this.Info.Source.Id);

            if (video == null)
            {
                this.Video = null;
            }
            else
            {
                this.Video = new VideoViewModel(video);

                this.EntitesView.Collection.AddRange(video.Entities
                    .Select(z => new EntityViewModel(z))
                    .GroupBy(v => v.Source.Resolution ?? "unknown")
                    .Select(g => new ObservableCollectionGroup<string, EntityViewModel>(g.Key, g)));
            }
        }

        public JasilyCollectionView<ObservableCollectionGroup<string, EntityViewModel>> EntitesView { get; private set; }
    }
}