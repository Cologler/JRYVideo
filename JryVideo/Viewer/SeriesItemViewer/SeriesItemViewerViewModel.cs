using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Common;

namespace JryVideo.Viewer.SeriesItemViewer
{
    public abstract class SeriesItemViewerViewModel : JasilyViewModel
    {
        public SeriesItemViewerViewModel()
        {
            this.VideosView = new JasilyCollectionView<VideoInfoViewModel>()
            {
                Filter = this.ItemFilter
            };
        }

        public JasilyCollectionView<VideoInfoViewModel> VideosView { get; private set; }

        protected virtual bool ItemFilter(VideoInfoViewModel obj)
        {
            return true;
        }

        public abstract Task RefreshAsync();
    }
}