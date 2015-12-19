using System.Collections.Generic;
using JryVideo.Common;
using JryVideo.Editors.CoverEditor;
using JryVideo.Editors.EntityEditor;
using JryVideo.Editors.SeriesEditor;
using JryVideo.Editors.VideoEditor;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JryVideo.Viewer.FilesViewer;

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

            var dlg = new CoverEditorWindow();
            var cover = await vm.TryGetCoverAsync();
            if (cover != null)
            {
                dlg.ViewModel.ModifyMode(cover);
                dlg.UpdateRadioButtonCheckedStatus();
            }
            else
            {
                dlg.ViewModel.CreateMode();
            }

            if (dlg.ShowDialog() == true)
            {
                await dlg.ViewModel.CommitAsync();
                vm.BeginUpdateCover();
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
                this.ViewModel.EntitesView.View.Refresh();
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

        private void CopyGuidButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as EntityViewModel;

            if (vm != null)
            {
                Clipboard.SetText(vm.Source.Id);
            }
        }

        private void EditEntityButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as EntityViewModel;

            if (vm != null)
            {
                var w = this.TryFindParent<MetroWindow>();

                var dlg = new EntityEditorWindow(this.ViewModel.Video.Source, vm.Source)
                {
                    Owner = w
                };

                if (dlg.ShowDialog() == true)
                {
                    vm.RefreshProperties();
                }
            }
        }

        private void GotoDoubanButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Info.NavigateToDouban();
        }

        private void GotoImdbButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Info.NavigateToImdb();
        }

        private async void DeleteEntityButton_OnClick(object sender, RoutedEventArgs e)
        {
            if ((await this.TryFindParent<MetroWindow>()
                .ShowMessageAsync("warnning", "are you sure you want to delete it?", MessageDialogStyle.AffirmativeAndNegative))
                == MessageDialogResult.Affirmative)
            {
                var entity = ((FrameworkElement)sender).DataContext as EntityViewModel;
                if (entity != null)
                {
                    await this.ViewModel.Video.RemoveAsync(entity);
                }
            }
        }

        private void CopyStringMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (menuItem != null &&
                menuItem.ItemsSource != null &&
                ReferenceEquals(menuItem, e.OriginalSource))
            {
                return;
            }

            var osItem = e.OriginalSource as MenuItem;

            if (osItem != null)
            {
                var str = osItem.DataContext as string;

                if (str == null)
                    str = osItem.Header as string;

                if (str != null)
                {
                    Clipboard.SetText(str);
                }
            }
        }

        private void EditSeriesMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var seriesViewModel = this.ViewModel.Info.SeriesView;

            if (seriesViewModel != null)
            {
                var dlg = new SeriesEditorWindow(seriesViewModel.Source)
                {
                    Owner = this.TryFindParent<Window>()
                };
                dlg.ShowDialog();
                seriesViewModel.RefreshProperties();
            }
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1 && this.ViewModel.Info.Cover != null)
            {
                var buffer = this.ViewModel.Info.Cover.BinaryData;
                if (buffer.Length > 0)
                {
                    string path;

                    do
                    {
                        path = Path.ChangeExtension(Path.GetTempFileName(), "jpg");
                    } while (File.Exists(path));

                    using (var file = File.Create(path))
                    {
                        file.Write(buffer);
                    }

                    Task.Run(() =>
                    {
                        using (var p = Process.Start(path))
                        {
                            p?.WaitForExit();
                        }

                        try
                        {
                            File.Delete(path);
                        }
                        catch
                        {
                            // ignored
                        }
                    });
                }
            }
        }

        private async void SearchOnEverything_OnClick(object sender, RoutedEventArgs e)
        {
            var entity = ((FrameworkElement)sender).DataContext as EntityViewModel;
            if (entity == null || entity.Source.Format == null) return;
            var items = await entity.SearchByEveryThingAsync();
            var dlg = new FilesViewerWindow(new FilesViewerViewModel());
            dlg.ViewModel.FilesView.Collection.AddRange(items.Select(z => new FileItemViewModel(z)));
            dlg.ShowDialog();
        }
    }
}
