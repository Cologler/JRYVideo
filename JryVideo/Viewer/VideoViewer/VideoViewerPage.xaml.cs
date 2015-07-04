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
using System.Windows.Navigation;
using System.Windows.Shapes;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Editors.CoverEditor;
using JryVideo.Editors.EntityEditor;
using JryVideo.Editors.VideoEditor;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace JryVideo.Viewer.VideoViewer
{
    /// <summary>
    /// VideoViewerPage.xaml 的交互逻辑
    /// </summary>
    public partial class VideoViewerPage : Page
    {
        private VideoViewerViewModel viewModel;

        public VideoViewerPage()
        {
            this.InitializeComponent();
        }

        public VideoViewerViewModel ViewModel
        {
            get { return this.viewModel; }
            set { this.DataContext = this.viewModel = value; }
        }

        internal static VideoViewerPage BuildPage(VideoInfoViewModel info)
        {
            var vm = new VideoViewerViewModel(info);

            return new VideoViewerPage()
            {
                ViewModel = vm
            };
        }

        private async void EditCover_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = this.ViewModel.Info;
            
            var cover = await vm.TryGetCoverAsync();
            if (cover != null)
            {
                var dlg = new CoverEditorWindow();
                dlg.ViewModel.ModifyMode(cover);
                dlg.UpdateRadioButtonCheckedStatus();

                if (dlg.ShowDialog() == true)
                {
                    await dlg.ViewModel.CommitAsync();
                    vm.BeginUpdateCover();
                }
            }
        }

        private async void AddEntityMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var w = this.TryFindParent<MetroWindow>();

            var dlg = new EntityEditorWindow(this.ViewModel.Video.Source)
            {
                Owner = w
            };

            if (dlg.ShowDialog() == true)
            {
                await this.ViewModel.ReloadVideoAsync();
            }
        }

        private void EditVideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new VideoEditorWindow(this.ViewModel.Info.SeriesView.Source, this.ViewModel.Info.Source)
            {
                Owner = this.TryFindParent<Window>()
            };

            dlg.ShowDialog();
            this.ViewModel.Info.Reload();
        }
    }
}
