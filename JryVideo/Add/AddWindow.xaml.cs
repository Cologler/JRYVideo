using System;
using System.Threading.Tasks;
using System.Windows;
using Jasily.Diagnostics;
using JryVideo.Add.VideoCreator;
using JryVideo.Common;
using JryVideo.Selectors.SeriesSelector;
using JryVideo.Viewer.SeriesItemViewer;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace JryVideo.Add
{
    /// <summary>
    /// SelectSeriesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddWindow : MetroWindow
    {
        private readonly SeriesSelectorPage seriesSelectorPage;
        private SeriesItemViewerPage seriesItemViewerPage;
        private VideoCreatorPage videoCreatorPage;

        public AddWindow()
        {
            this.InitializeComponent();

            this.seriesSelectorPage = new SeriesSelectorPage();
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Window.SourceInitialized"/> 事件。
        /// </summary>
        /// <param name="e">一个 <see cref="T:System.EventArgs"/>，其中包含事件数据。</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.NavigateToSeriesSelectorPage();
        }

        private async void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selected = this.seriesSelectorPage.SelectorViewModel.Items.Selected;

            if (selected == null)
            {
                await this.ShowMessageAsync("error", "must select a series");
                return;
            }

            if (this.ContentFrame.Content == this.seriesSelectorPage)
            {
                this.NavigateToSeriesItemViewerPage(selected);
                return;
            }
            else if (this.ContentFrame.Content == this.seriesItemViewerPage)
            {
                await this.NavigateToCreateVideoPage(selected);
                return;
            }
        }

        private void NavigateToSeriesSelectorPage()
        {
            this.TitleTextBlock.Text = "select series";
            this.LastButton.Visibility = Visibility.Hidden;

            JasilyDebug.Pointer();
            this.ContentFrame.Navigate(this.seriesSelectorPage);
            JasilyDebug.Pointer();
        }

        private void NavigateToSeriesItemViewerPage(SeriesViewModel series)
        {
            this.TitleTextBlock.Text = "sure video was not exists";
            this.LastButton.Visibility = this.NextButton.Visibility = Visibility.Visible;

            if (this.seriesItemViewerPage == null || this.seriesItemViewerPage.ViewModel.Source != series.Source)
            {
                this.seriesItemViewerPage = new SeriesItemViewerPage(series.Source);
            }

            this.ContentFrame.Navigate(this.seriesItemViewerPage);
        }

        private async Task NavigateToCreateVideoPage(SeriesViewModel series)
        {
            this.TitleTextBlock.Text = "create video";
            this.NextButton.Visibility = Visibility.Hidden;

            if (this.videoCreatorPage == null || this.videoCreatorPage.CreatorViewModel.Source != series.Source)
            {
                this.videoCreatorPage = new VideoCreatorPage(series.Source);
            }

            this.ContentFrame.Navigate(this.videoCreatorPage);

            await this.videoCreatorPage.CreatorViewModel.LoadAsync();
        }

        private void LastButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ContentFrame.CanGoBack)
            {
                if (this.ContentFrame.Content is VideoCreatorPage)
                {
                    this.TitleTextBlock.Text = "sure video was not exists";
                }
                else if (this.ContentFrame.Content is SeriesItemViewerPage)
                {
                    this.TitleTextBlock.Text = "select series";
                }

                this.ContentFrame.GoBack();
            }

            this.NextButton.Visibility = Visibility.Visible;
            this.LastButton.Visibility = this.ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
