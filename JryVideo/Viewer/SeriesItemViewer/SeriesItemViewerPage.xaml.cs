using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JryVideo.Common;
using JryVideo.Editors.CoverEditor;
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
            this.ViewModel = new SingleSeriesItemViewerViewModel(series);
            this.ViewModel.LoadAsync();
        }
    }
}
