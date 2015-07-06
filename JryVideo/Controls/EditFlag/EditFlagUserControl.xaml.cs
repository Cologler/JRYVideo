﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using JryVideo.Model;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

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
            if (this.ViewModel.Value.IsNullOrWhiteSpace())
            {
                await this.TryFindParent<MetroWindow>().ShowMessageAsync("error", "name can not be empty.");
                return;
            }

            await this.ViewModel.CommitAsync(this.FlagType.Value);
        }
    }
}