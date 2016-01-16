using JryVideo.Common;
using JryVideo.Viewer.VideoViewer;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Enums;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Main
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public const string Caption = "JRY VIDEO";
        private MainPage MainPage;
        private int messageId;

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
            this.CaptionTextBlock.Text = Caption;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                App.UserConfigChanged += this.App_UserConfigChanged;

                this.MainPage = new MainPage();
                this.MainPage.VideoSelected += this.MainPage_VideoSelected;
                this.MainFrame.Navigate(this.MainPage);
                this.BeginRefresh();
            }
        }

        private void App_UserConfigChanged(object sender, Configs.UserConfig e)
        {
            if (e != null)
            {
                this.ShowStatusMessage("read user config successd");
            }
            else
            {
                this.ShowStatusMessage("read user config failed");
            }
        }

        private async void ShowStatusMessage(string msg)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.ShowStatusMessage(msg));
                return;
            }

            var id = ++this.messageId;
            this.StatusTextBlock.Text = msg;
            await Task.Delay(5000);
            if (this.messageId == id)
            {
                this.StatusTextBlock.Text = string.Empty;
            }
        }

        private void BeginRefresh()
        {
            Task.Run(async () =>
            {
                var lastDay = DateTime.Now;
                while (true)
                {
                    var bs = Process.GetCurrentProcess().WorkingSet64.GetByteSize();
                    if (bs.OriginValue > 1024 * 1024 * 500)
                    {
                        GC.Collect();
                        bs = Process.GetCurrentProcess().WorkingSet64.GetByteSize();
                    }
                    await this.Dispatcher.BeginInvoke(() => this.MemoryTextBlock.Text = bs.ToString());
                    var now = DateTime.Now;
                    if (now.Day != lastDay.Day)
                    {
                        lastDay = now;
                        await this.Dispatcher.BeginInvoke(() => this.MainPage?.Refresh());
                    }
                    await Task.Delay(1000);
                }
            });
        }

        void MainPage_VideoSelected(object sender, VideoInfoViewModel e) => this.NavigateToVideoViewerPage(e);

        private async void NavigateToVideoViewerPage(VideoInfoViewModel info)
        {
            var page = VideoViewerPage.BuildPage(info);
            page.GoBackButton.Click += this.VideoViewerPage_GoBackButton_Click;
            page.ViewModel.InfoView.SeriesView.PropertiesRefreshed += this.InfoView_PropertiesRefreshed;
            this.CaptionTextBlock.Text = Caption + " | " + info.SeriesView.DisplayNameFirstLine;
            this.MainFrame.Navigate(page);
            await page.ViewModel.LoadAsync();
        }

        private void InfoView_PropertiesRefreshed(object sender, EventArgs e)
        {
            this.CaptionTextBlock.Text = Caption + " | " + ((SeriesViewModel)sender).DisplayNameFirstLine;
        }

        void VideoViewerPage_GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).Click -= this.VideoViewerPage_GoBackButton_Click;

            var page = (VideoViewerPage)this.MainFrame.Content;
            Debug.Assert(page != null);
            page.ViewModel.InfoView.SeriesView.PropertiesRefreshed -= this.InfoView_PropertiesRefreshed;
            page.ViewModel.Flush();
            this.CaptionTextBlock.Text = Caption;
            Debug.Assert(this.MainFrame.CanGoBack);
            this.MainFrame.GoBack();
            this.MainPage.RefreshVideo(page.ViewModel.InfoView);
        }
    }
}
