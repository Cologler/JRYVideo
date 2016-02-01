using JryVideo.Controls.SelectVideo;
using JryVideo.Model;
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

        public async void SetSeries(JrySeries series)
        {
            this.ViewModel.Source = series;
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            await this.ViewModel.RefreshAsync();
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }
    }
}
