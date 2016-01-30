using JryVideo.Common.Dialogs;
using JryVideo.Core;
using System.Collections.Generic;
using System.Windows;

namespace JryVideo.Selectors.WebImageSelector
{
    /// <summary>
    /// WebImageSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WebImageSelectorWindow
    {
        public WebImageSelectorViewModel ViewModel { get; }
            = new WebImageSelectorViewModel();

        public WebImageSelectorWindow()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// return null if none select
        /// </summary>
        /// <returns></returns>
        public static string StartSelect(Window parent, IEnumerable<string> urls)
        {
            var dlg = new WebImageSelectorWindow() { Owner = parent };
            dlg.ViewModel.Load(urls);
            return dlg.ShowDialog() == true ? dlg.ImagesListView.SelectedItem as string : null;
        }

        public static string StartSelectByImdbId(Window parent, string imdbId)
        {
            var client = JryVideoCore.Current.TheTVDBClient;
            if (client == null)
            {
                parent.ShowJryVideoMessage("error", "TheTVDB init failed, try again.");
                return null;
            }
            var dlg = new WebImageSelectorWindow() { Owner = parent };
            dlg.ViewModel.BeginLoadPosterByImdbId(client, imdbId);
            return dlg.ShowDialog() == true ? dlg.ImagesListView.SelectedItem as string : null;
        }

        public static string StartSelectFanartByImdbId(Window parent, string index, params string[] imdbIds)
        {
            var client = JryVideoCore.Current.TheTVDBClient;
            if (client == null)
            {
                parent.ShowJryVideoMessage("error", "TheTVDB init failed, try again.");
                return null;
            }
            var dlg = new WebImageSelectorWindow() { Owner = parent };
            dlg.ViewModel.BeginLoadFanartByImdbId(client, index, imdbIds);
            return dlg.ShowDialog() == true ? dlg.ImagesListView.SelectedItem as string : null;
        }

        private void AcceptButton_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
