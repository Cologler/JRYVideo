using System;
using System.Enums;
using System.Windows;
using System.Windows.Controls;
using JryVideo.Common.Dialogs;

namespace JryVideo.Controls.EditFlag
{
    /// <summary>
    /// EditFlagUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class EditFlagUserControl : UserControl
    {
        public EditFlagViewModel ViewModel { get; }

        public EditFlagUserControl()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel = new EditFlagViewModel();
        }

        private async void CommitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.Value.IsNullOrWhiteSpace())
            {
                this.ShowJryVideoMessage("ERROR", "name can not be empty.");
                return;
            }

            this.ViewModel.Value = this.ViewModel.Value.Trim();

            if (this.ViewModel.Action == ObjectChangedAction.Modify)
            {
                if (this.ViewModel.Value == this.ViewModel.OldValue)
                {
                    this.ShowJryVideoMessage("ERROR", "old name was same with new name.");
                    return;
                }
            }

            await this.ViewModel.CommitAsync();
        }
    }
}
