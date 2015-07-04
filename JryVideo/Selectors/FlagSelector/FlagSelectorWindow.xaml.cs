using System;
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
using System.Windows.Shapes;
using JryVideo.Common;
using JryVideo.Model;
using MahApps.Metro.Controls;

namespace JryVideo.Selectors.FlagSelector
{
    /// <summary>
    /// FlagSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FlagSelectorWindow : MetroWindow
    {
        public FlagSelectorViewModel ViewModel { get; private set; }

        public FlagSelectorWindow()
        {
            this.InitializeComponent();
        }

        public FlagSelectorWindow(JryFlagType type, IEnumerable<string> readySelected = null)
            : this()
        {
            this.TitleTextBlock.Text = String.Format(
                Properties.Resources.FlagSelectorWindow_Title_Format,
                type.GetLocalizeString());

            this.DataContext = this.ViewModel = new FlagSelectorViewModel(type, readySelected ?? Enumerable.Empty<string>());
            this.EditFlagUserControl.FlagType = type;
            this.EditFlagUserControl.ViewModel.Created += this.ViewModel.EditFlagUserControl_ViewModel_Created;
            this.ViewModel.LoadAsync();
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
    }
}
