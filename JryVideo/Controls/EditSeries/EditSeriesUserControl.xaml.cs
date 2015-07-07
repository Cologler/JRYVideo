using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace JryVideo.Controls.EditSeries
{
    /// <summary>
    /// EditSeriesUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class EditSeriesUserControl : UserControl
    {
        public EditSeriesViewModel ViewModel { get; private set; }

        public EditSeriesUserControl()
        {
            this.InitializeComponent();

            this.DataContext = this.ViewModel = new EditSeriesViewModel();
        }

        void ViewModel_FindErrorMessages(object sender, object e)
        {
            if (this.Dispatcher.CheckAccessOrBeginInvoke(this.ViewModel_FindErrorMessages, sender, e))
            {
                this.TryFindParent<MetroWindow>().ShowMessageAsync(Properties.Resources.EditSeries_InvalidInput_Title, "");
            }
        }

        private async void CommitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.Names.IsNullOrWhiteSpace())
            {
                await this.TryFindParent<MetroWindow>().ShowMessageAsync(Properties.Resources.EditSeries_InvalidInput_Title, "name can not be empty.");
                return;
            }

            if (await this.ViewModel.CommitAsync() != null)
            {
                
            }
        }

        private async void LoadFromDoubanButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.LoadDoubanAsync();
        }
    }
}
