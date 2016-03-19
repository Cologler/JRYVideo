using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using JryVideo.Selectors.FlagSelector;
using MahApps.Metro.Controls;

namespace JryVideo.Controls.SelectFlag
{
    /// <summary>
    /// SelectFlagControl.xaml 的交互逻辑
    /// </summary>
    public partial class SelectFlagControl : UserControl
    {
        public SelectFlagControl()
        {
            this.InitializeComponent();
        }

        private void SelectButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as SelectFlagViewModel;
            Debug.Assert(dc != null);
            var dlg = new FlagSelectorWindow(dc.Type, dc.Collection)
            {
                Owner = this.TryFindParent<Window>()
            };

            if (dlg.ShowDialog() == true)
            {
                dc.Collection.Clear();
                dc.Collection.AddRange(dlg.ViewModel.SelectedItems.Select(z => z.Source.Value));
            }
        }

        private void CopyMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var flag = ((FrameworkElement)sender).DataContext as string;
            Debug.Assert(flag != null);
            Clipboard.SetText(flag);
        }

        private void RemoveMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext as SelectFlagViewModel;
            Debug.Assert(dc != null);
            var flag = ((FrameworkElement)sender).DataContext as string;
            Debug.Assert(flag != null);
            dc.Collection.Remove(flag);
        }
    }
}
