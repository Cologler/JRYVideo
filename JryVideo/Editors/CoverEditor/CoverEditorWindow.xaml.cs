using JryVideo.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using JryVideo.Common.Dialogs;

namespace JryVideo.Editors.CoverEditor
{
    /// <summary>
    /// EditCoverWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CoverEditorWindow : MetroWindow
    {
        public CoverEditorViewModel ViewModel { get; private set; }

        public CoverEditorWindow()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel = new CoverEditorViewModel();
        }

        public CoverEditorWindow(CoverEditorViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel = viewModel;
        }

        public void UpdateRadioButtonCheckedStatus()
        {
            switch (this.ViewModel.CoverSourceType)
            {
                case JryCoverSourceType.Local:
                    this.ChooseRadioButton.IsChecked = true;
                    break;

                case JryCoverSourceType.Uri:
                    this.UrlRadioButton.IsChecked = true;
                    break;

                case JryCoverSourceType.Douban:
                    this.DoubanRadioButton.IsChecked = true;
                    break;

                case JryCoverSourceType.Imdb:
                    this.ImdbRadioButton.IsChecked = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DoubanRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CoverSourceType = JryCoverSourceType.Douban;
        }

        private void UrlRadioButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CoverSourceType = JryCoverSourceType.Uri;
        }

        private void ChooseRadioButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            this.ChooseButton.IsEnabled = false;
        }

        private void ChooseRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            this.ChooseButton.IsEnabled = true;
            this.ViewModel.CoverSourceType = JryCoverSourceType.Local;
        }

        private void ChooseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();

            dlg.Filter = "(*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png";

            if (dlg.ShowDialog() == true)
            {
                this.ChooseRadioButton.IsChecked = true;
                this.ViewModel.BinaryData = dlg.OpenFile().ToArray();
            }
        }

        private async void LoadFromUrlButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!await this.ViewModel.LoadFromUrlAsync())
            {
                this.ShowJryVideoMessage("error", "load failed");
            }
            else
            {
                this.UrlRadioButton.IsChecked = true;
            }
        }

        private async void LoadFromDoubanButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!await this.ViewModel.LoadFromDoubanAsync())
            {
                this.ShowJryVideoMessage("error", "load failed");
            }
            else
            {
                this.DoubanRadioButton.IsChecked = true;
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
            else if (result == true)
            {
                this.ImdbRadioButton.IsChecked = true;
            }
        }

        private void ImdbRadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.CoverSourceType = JryCoverSourceType.Imdb;
        }
    }
}
