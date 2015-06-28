using System.ComponentModel;
using System.Threading.Tasks;
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

            this.Video = video == null ? null : new VideoViewModel(video);
        }

    }
}