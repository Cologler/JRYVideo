using System;
using System.Enums;
using System.Windows;
using System.Windows.Controls;
using JryVideo.Model;
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
            this.ViewModel.FindErrorMessages += this.ViewModel_FindErrorMessages;
        }

        void ViewModel_FindErrorMessages(object sender, string[] e)
        {
            if (this.Dispatcher.CheckAccessOrBeginInvoke(this.ViewModel_FindErrorMessages, sender, e))
            {
                this.TryFindParent<MetroWindow>().ShowMessageAsync(Properties.Resources.EditSeries_Error_Title, e.AsLines());
            }
        }

        public void SetCreate()
        {
            this.ViewModel.Action = ObjectChangedAction.Create;
        }

        public void SetModify(JrySeries series)
        {
            this.ViewModel.Action = ObjectChangedAction.Modify;
            this.ViewModel.Source = series;
        }

        private async void CommitButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.CommitAsync();
        }

        private async void LoadFromDoubanButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.LoadDoubanAsync();
        }
    }
}
