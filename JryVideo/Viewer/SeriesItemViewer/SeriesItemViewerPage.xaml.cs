using JryVideo.Controls.SelectVideo;
using System.Windows.Controls;

namespace JryVideo.Viewer.SeriesItemViewer
{
    /// <summary>
    /// SeriesItemViewerPage.xaml 的交互逻辑
    /// </summary>
    public partial class SeriesItemViewerPage : Page
    {
        public SelectVideoViewModel ViewModel { get; }
            = new SelectVideoViewModel();

        public SeriesItemViewerPage()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }
    }
}
