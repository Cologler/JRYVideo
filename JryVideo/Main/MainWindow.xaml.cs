using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using JryVideo.Common;
using JryVideo.Viewer.VideoViewer;
using MahApps.Metro.Controls;

namespace JryVideo.Main
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainPage MainPage;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Window.SourceInitialized"/> 事件。
        /// </summary>
        /// <param name="e">一个 <see cref="T:System.EventArgs"/>，其中包含事件数据。</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.MainPage = new MainPage();
                this.MainPage.VideoSelected += this.MainPage_VideoSelected;

                this.NavigateToMainPage();
            }
        }

        void MainPage_VideoSelected(object sender, VideoInfoViewModel e)
        {
            this.NavigateToVideoViewerPage(e);
        }

        private void NavigateToMainPage()
        {
            this.MainFrame.Navigate(this.MainPage);
        }

        private async void NavigateToVideoViewerPage(VideoInfoViewModel info)
        {
            var page = VideoViewerPage.BuildPage(info);
            page.GoBackButton.Click += this.VideoViewerPage_GoBackButton_Click;
            this.MainFrame.Navigate(page);
            await page.ViewModel.LoadAsync();
        }

        void VideoViewerPage_GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button) sender).Click -= this.VideoViewerPage_GoBackButton_Click;

            if (this.MainFrame.CanGoBack)
            {
                this.MainFrame.GoBack();
            }
        }
    }
}
