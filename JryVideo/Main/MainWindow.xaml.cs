using Jasily.Desktop.Windows.Navigation;
using JryVideo.Common;
using JryVideo.Viewer.VideoViewer;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Enums;
using System.Threading.Tasks;
using System.Windows;

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

        public NavigationStatus MainFrameNavigationStatus { get; private set; }

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
                this.MainFrameNavigationStatus = new NavigationStatus(this.MainFrame.NavigationService);

                App.UserConfigChanged += this.App_UserConfigChanged;

                this.MainPage = new MainPage();
                this.MainPage.VideoSelected += this.MainPage_VideoSelected;
                this.MainFrame.Navigate(this.MainPage);
                OnShowMessage += this.MainWindow_OnShowMessage;
                this.BeginRefresh();
            }
        }

        private void MainWindow_OnShowMessage(object sender, string e)
        {
            Debug.Assert(e != null);
            this.ShowStatusMessage(e);
        }

        private void App_UserConfigChanged(object sender, Configs.UserConfig e)
            => this.ShowStatusMessage(e != null ? "read user config successd" : "read user config failed");

        private static event EventHandler<string> OnShowMessage;

        public static void ShowMessage(string msg) => OnShowMessage?.Invoke(null, msg);

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
                    using (var p = Process.GetCurrentProcess())
                    {
                        var bs = p.WorkingSet64.GetByteSize();
                        if (bs.OriginValue > 1024 * 1024 * 500)
                        {
                            GC.Collect();
                            bs = p.WorkingSet64.GetByteSize();
                        }
                        await this.Dispatcher.BeginInvoke(() => this.MemoryTextBlock.Text = bs.ToString());
                    }
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
            page.ClickedGoBack += this.VideoViewerPage_ClickedGoBack;
            page.ClickedOtherVideo += this.VideoViewerPage_ClickedOtherVideo;

            page.ViewModel.InfoView.SeriesView.PropertiesRefreshed += this.InfoView_PropertiesRefreshed;
            this.CaptionTextBlock.Text = Caption + " | " + info.SeriesView.NameViewModel.FirstLine;
            this.MainFrame.Navigate(page);
            await page.ViewModel.LoadAsync();
        }

        private void VideoViewerPage_ClickedOtherVideo(object sender, VideoInfoViewModel e)
        {
            this.nextVideo = e;
            this.MainFrame.Navigated += this.MainFrame_NextVideo;
            this.ExitVideoViewerPage((VideoViewerPage)sender);
        }

        private void MainFrame_NextVideo(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            this.MainFrame.Navigated -= this.MainFrame_NextVideo;
            var v = this.nextVideo;
            this.nextVideo = null;
            if (v != null) this.NavigateToVideoViewerPage(v);
        }

        private VideoInfoViewModel nextVideo;

        private void ExitVideoViewerPage(VideoViewerPage page)
        {
            page.ClickedGoBack -= this.VideoViewerPage_ClickedGoBack;
            page.ClickedOtherVideo -= this.VideoViewerPage_ClickedOtherVideo;

            Debug.Assert(page != null);
            page.ViewModel.InfoView.SeriesView.PropertiesRefreshed -= this.InfoView_PropertiesRefreshed;
            page.ViewModel.Flush();
            this.CaptionTextBlock.Text = Caption;
            Debug.Assert(this.MainFrame.CanGoBack);
            this.MainFrame.GoBack();
            this.MainPage.RefreshVideo(page.ViewModel.InfoView);
        }

        private void VideoViewerPage_ClickedGoBack(object sender, EventArgs e)
            => this.ExitVideoViewerPage((VideoViewerPage)sender);

        private void InfoView_PropertiesRefreshed(object sender, EventArgs e)
        {
            this.CaptionTextBlock.Text = Caption + " | " + ((SeriesViewModel)sender).NameViewModel.FirstLine;
        }
    }
}
