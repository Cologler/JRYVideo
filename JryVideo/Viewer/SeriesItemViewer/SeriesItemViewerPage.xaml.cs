using System.ComponentModel;
using System.Windows.Controls;
using JryVideo.Model;

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
            set { this.DataContext = this.viewModel = value; }
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
                this.ViewModel.RefreshAsync();
            }
        }
    }
}
