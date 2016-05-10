using System.Windows.Controls;
using JryVideo.Common;
using JryVideo.Controls.SelectVideo;

namespace JryVideo.Viewer.SeriesItemViewer
{
    /// <summary>
    /// SeriesItemViewerPage.xaml 的交互逻辑
    /// </summary>
    public partial class SeriesItemViewerPage : Page
    {
        public SeriesItemViewerPage()
        {
            this.InitializeComponent();
        }

        public void Initialize(SeriesViewModel series, string defaultId = null)
            => this.SelectVideoControl.Initialize(series, defaultId);

        public SeriesViewModel GetCurrentSeriesViewModel()
            => this.SelectVideoControl.ViewModel.Series;
    }
}
