using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JryVideo.Common;
using JryVideo.Common.Dialogs;
using JryVideo.Core.Managers;
using JryVideo.Editors.FlagEditor;
using JryVideo.Model;
using MahApps.Metro.Controls.Dialogs;

namespace JryVideo.Selectors.FlagSelector
{
    /// <summary>
    /// FlagSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FlagSelectorWindow
    {
        public FlagSelectorViewModel ViewModel { get; }

        public FlagSelectorWindow()
        {
            this.InitializeComponent();
        }

        public FlagSelectorWindow(JryFlagType type, IEnumerable<string> readySelected = null)
            : this()
        {
            this.TitleTextBlock.Text = string.Format(
                Properties.Resources.FlagSelectorWindow_Title_Format,
                type.GetLocalizeString());

            this.DataContext = this.ViewModel = new FlagSelectorViewModel(type);
            this.EditFlagUserControl.ViewModel.FlagType = type;
            this.EditFlagUserControl.ViewModel.CreateMode();
            this.EditFlagUserControl.ViewModel.Creating += this.EditFlagUserControl_ViewModel_Creating;
            this.EditFlagUserControl.ViewModel.Created += this.ViewModel.EditFlagUserControl_ViewModel_Created;
            if (readySelected != null)
            {
                this.ViewModel.SelectedStrings.AddRange(readySelected);
            }
#pragma warning disable 4014
            this.ViewModel.LoadAsync();
#pragma warning restore 4014
        }

        public void EditFlagUserControl_ViewModel_Creating(object sender, RequestActionEventArgs<JryFlag> e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (this.ViewModel.Items.Collection.FirstOrDefault(z => z.Source.Value == e.Arg.Value) != null)
                {
                    e.IsAccept = false;
                    this.ShowJryVideoMessage("ERROR", $"the '{e.Arg.Value}' was ready in {this.ViewModel.Type.GetLocalizeString()}");
                }
            });
        }

        private void AccetpButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void SourceItemPanel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ViewModel.SelectItem(((FrameworkElement)sender).DataContext as FlagViewModel);
        }

        private void SelectedItemPanel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ViewModel.UnselectItem(((FrameworkElement)sender).DataContext as FlagViewModel);
        }

        private async void EditFlagMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var flagViewModel = ((FrameworkElement)sender).DataContext as FlagViewModel;
            if (flagViewModel == null) return;
            var win = new FlagEditorWindow() { Owner = this };
            Debug.Assert(FlagManager.CanReplace(flagViewModel.Source.Type));
            win.InitializeEditor(flagViewModel.Source);
            if (win.ShowDialog() == true)
            {
                this.ViewModel.Sync();
                await this.ViewModel.LoadAsync();
            }
        }

        private async void DeleteFlagMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (await this.ShowMessageAsync("WARN", "are you sure you want to delete it?\r\n(whitout delete entity)",
                MessageDialogStyle.AffirmativeAndNegative)
                == MessageDialogResult.Affirmative)
            {
                var flagViewModel = ((FrameworkElement)sender).DataContext as FlagViewModel;
                if (flagViewModel != null)
                {
                    await this.ViewModel.DeleteItemAsync(flagViewModel);
                }
            }
        }
    }
}
