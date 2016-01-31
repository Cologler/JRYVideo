using JryVideo.Model;
using System.ComponentModel;
using System.Windows.Controls;

namespace JryVideo.Viewer.SeriesItemViewer
{
    /// <summary>
    /// SeriesItemViewerPage.xaml 的交互逻辑
    /// </summary>
    public partial class SeriesItemViewerPage : Page
    {
        private SingleSeriesItemViewerViewModel viewModel;

        /// <summary>
        /// should set by parent
        /// </summary>
        public SingleSeriesItemViewerViewModel ViewModel
        {
            get { return this.viewModel; }
            private set { this.DataContext = this.viewModel = value; }
        }

        public SeriesItemViewerPage()
        {
            this.InitializeComponent();
        }

        public SeriesItemViewerPage(JrySeries series)
            : this()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.ViewModel = new SingleSeriesItemViewerViewModel(series);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                this.ViewModel.RefreshAsync();
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            }
        }
    }
}
