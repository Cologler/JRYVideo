using System;
using System.Threading.Tasks;
using System.Windows;
using JryVideo.Add.SelectSeries;
using JryVideo.Add.SeriesSelector;
using JryVideo.Add.VideoCreator;
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
        private SeriesSelectorPage seriesSelectorPage;
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
                await this.NavigateToSelectVideoPage(selected);
                return;
            }
        }

        private void NavigateToSeriesSelectorPage()
        {
            this.Title.Text = "select series";
            this.LastButton.Visibility = Visibility.Hidden;

            this.ContentFrame.Navigate(this.seriesSelectorPage);
        }

        private void NavigateToSeriesItemViewerPage(SeriesViewModel series)
        {
            this.Title.Text = "sure video was not exists";
            this.LastButton.Visibility = this.NextButton.Visibility = Visibility.Visible;

            if (this.seriesItemViewerPage == null || this.seriesItemViewerPage.ViewModel.Source != series.Source)
            {
                this.seriesItemViewerPage = new SeriesItemViewerPage(series.Source);
            }

            this.ContentFrame.Navigate(this.seriesItemViewerPage);
        }

        private async Task NavigateToSelectVideoPage(SeriesViewModel series)
        {
            this.Title.Text = "create video";
            this.NextButton.Visibility = Visibility.Hidden;

            if (this.videoCreatorPage == null || this.videoCreatorPage.CreatorViewModel.Source != series.Source)
            {
                this.videoCreatorPage = new VideoCreatorPage(series.Source);
            }

            if (this.seriesSelectorPage.EditSeriesUserControl.ViewModel.DoubanMovie != null)
            {

            }

            this.ContentFrame.Navigate(this.videoCreatorPage);

            await this.videoCreatorPage.CreatorViewModel.LoadAsync();
        }

        private async void LastButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ContentFrame.Content == this.seriesItemViewerPage)
            {
                this.NavigateToSeriesSelectorPage();
                return;
            }
            else if (this.ContentFrame.Content == this.videoCreatorPage)
            {
                var selected = this.seriesSelectorPage.SelectorViewModel.Items.Selected;

                if (selected == null)
                {
                    this.NavigateToSeriesSelectorPage();
                }
                else
                {
                    this.NavigateToSeriesItemViewerPage(selected);
                }
            }
        }
    }
}
