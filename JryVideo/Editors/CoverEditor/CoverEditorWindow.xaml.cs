using System;
using System.IO;
using System.Windows;
using JryVideo.Common.Dialogs;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace JryVideo.Editors.CoverEditor
{
    /// <summary>
    /// EditCoverWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CoverEditorWindow : MetroWindow
    {
        public CoverEditorViewModel ViewModel { get; private set; }

        [Obsolete("must use CoverEditorWindow(CoverEditorViewModel viewModel)")]
        public CoverEditorWindow()
        {
            this.InitializeComponent();
        }

        public CoverEditorWindow(CoverEditorViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel = viewModel;
        }

        private void ChooseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();

            dlg.Filter = "(*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png";

            if (dlg.ShowDialog() == true)
            {
                this.ViewModel.BinaryData = dlg.OpenFile().ToArray();
            }
        }

        private async void LoadFromUrlButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!await this.ViewModel.LoadFromUrlAsync())
            {
                this.ShowJryVideoMessage("error", "load failed");
            }
        }

        private async void LoadFromDoubanButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!await this.ViewModel.LoadFromDoubanAsync())
            {
                this.ShowJryVideoMessage("error", "load failed");
            }
        }

        private async void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.ImageViewModel != null)
            {
                this.DialogResult = true;
            }
            else
            {
                await this.ShowMessageAsync("error", "don't have image.");
            }
        }

        private async void LoadFromImdbButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = await this.ViewModel.LoadFromImdbIdAsync(this);
            if (result == false)
            {
                this.ShowJryVideoMessage("error", "load failed");
            }
        }
    }
}
