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
        public EditSeriesViewModel ViewModel { get; } = new EditSeriesViewModel();

        public EditSeriesUserControl()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        private async void CommitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.NamesViewModel.Names.IsNullOrWhiteSpace())
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
