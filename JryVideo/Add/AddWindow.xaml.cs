using System;
using System.Windows;
using JryVideo.Add.SelectSeries;
using JryVideo.Add.SelectVideo;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace JryVideo.Add
{
    /// <summary>
    /// SelectSeriesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddWindow : MetroWindow
    {
        private SelectSeriesPage SelectSeriesPage { get; set; }

        private SelectVideoPage SelectVideoPage { get; set; }

        public AddWindow()
        {
            this.InitializeComponent();

            this.SelectSeriesPage = new SelectSeriesPage();
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Window.SourceInitialized"/> 事件。
        /// </summary>
        /// <param name="e">一个 <see cref="T:System.EventArgs"/>，其中包含事件数据。</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.ContentFrame.Navigate(this.SelectSeriesPage);
        }

        private async void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.SelectSeriesPage.ViewModel.Selected == null)
            {
                await this.ShowMessageAsync("error", "must select a series");
                return;
            }

            if (this.SelectVideoPage == null)
            {
                this.SelectVideoPage = new SelectVideoPage();
            }

            await this.SelectVideoPage.SetSourceAsync(this.SelectSeriesPage.ViewModel.Selected.Source);

            this.ContentFrame.Navigate(this.SelectVideoPage);
        }
    }
}
