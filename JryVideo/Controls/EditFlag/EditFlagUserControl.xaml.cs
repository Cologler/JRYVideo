using JryVideo.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Controls.EditFlag
{
    /// <summary>
    /// EditFlagUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class EditFlagUserControl : UserControl
    {
        public EditFlagViewModel ViewModel { get; private set; }

        public EditFlagUserControl()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel = new EditFlagViewModel();
        }

        public JryFlagType? FlagType { get; set; }

        private async void CommitButton_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.Assert(this.FlagType.HasValue);

            if (this.ViewModel.Value.IsNullOrWhiteSpace())
            {
                await this.TryFindParent<MetroWindow>().ShowMessageAsync("error", "name can not be empty.");
                return;
            }

            this.ViewModel.Value = this.ViewModel.Value.Trim();

            await this.ViewModel.CommitAsync(this.FlagType.Value);
        }
    }
}
