using Jasily.Windows.Data;
using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.Common;
using System.Threading.Tasks;

namespace JryVideo.Viewer.SeriesItemViewer
{
    public abstract class SeriesItemViewerViewModel : BaseSelectorViewModel<VideoInfoViewModel, JryVideoInfo>
    {
        public JasilyCollectionView<VideoInfoViewModel> VideosView => base.Items;

        public abstract Task RefreshAsync();
    }
}