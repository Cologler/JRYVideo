using Jasily.Windows.Data;
using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.Common;

namespace JryVideo.Viewer.SeriesItemViewer
{
    public abstract class SeriesItemViewerViewModel : BaseSelectorViewModel<VideoInfoViewModel, JryVideoInfo>
    {
        public JasilyCollectionView<VideoInfoViewModel> VideosView => this.Items;
    }
}