﻿using System;
using System.IO;
using System.Net;
using System.Windows;
using JryVideo.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace JryVideo.EditCover
{
    /// <summary>
    /// EditCoverWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditCoverWindow : MetroWindow
    {
        public EditCoverViewModel ViewModel { get; private set; }

        public EditCoverWindow()
        {
            this.InitializeComponent();
        }

        public EditCoverWindow(JryCover cover)
            : this()
        {
            this.DataContext = this.ViewModel = new EditCoverViewModel(cover);
            
            switch (cover.CoverSourceType)
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
                this.ViewModel.BinaryData = dlg.OpenFile().ToArray();
            }
        }

        private async void LoadFromUrlButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!await this.ViewModel.LoadFromUrlAsync())
                MessageBox.Show("load failed.");
        }

        private async void LoadFromDoubanButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!await this.ViewModel.LoadFromDoubanAsync())
                MessageBox.Show("load failed.");
        }

        private async void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.AcceptAsync();
            this.DialogResult = true;
        }
    }
}
