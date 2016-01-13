using JryVideo.Common;
using JryVideo.Editors.CoverEditor;
using JryVideo.Editors.EntityEditor;
using JryVideo.SearchEngine;
using JryVideo.Viewer.FilesViewer;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JryVideo.Viewer.VideoViewer
{
    /// <summary>
    /// VideoViewerPage.xaml 的交互逻辑
    /// </summary>
    public partial class VideoViewerPage : Page
    {
        private static readonly ISearchEngine[] Engines;

        static VideoViewerPage()
        {
            var @interface = typeof(ISearchEngine);
            Engines = typeof(VideoViewerPage).Assembly.DefinedTypes
                .Where(z => @interface.IsAssignableFrom(z))
                .Where(z => z.IsSealed)
                .Select(z => (ISearchEngine)Activator.CreateInstance(z))
                .OrderBy(z => z.Order)
                .ToArray();
        }

        public VideoViewerPage()
        {
            this.InitializeComponent();
        }

        public VideoViewerViewModel ViewModel { get; private set; }

        internal static VideoViewerPage BuildPage(VideoInfoViewModel info)
        {
            var vm = new VideoViewerViewModel(info);

            var page = new VideoViewerPage()
            {
                ViewModel = vm,
                DataContext = vm
            };

            Engines.ForEach(z => AppendToContextMenu(page.SeriesContextMenu, z));
            return page;
        }

        private static void AppendToContextMenu(ContextMenu menu, ISearchEngine engine)
        {
            var item = new MenuItem()
            {
                Header = "search on " + engine.Name,
                Tag = engine
            };
            item.Click += SearchStringOnEngine_OnClick;
            item.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("InfoView.SeriesView.Source.Names"));
            menu.Items.Add(item);
        }

        private static void SearchStringOnEngine_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (menuItem?.ItemsSource != null && ReferenceEquals(menuItem, e.OriginalSource))
            {
                return;
            }

            var osItem = e.OriginalSource as MenuItem;

            if (osItem != null)
            {
                var str = osItem.DataContext as string ?? osItem.Header as string;

                if (str != null)
                {
                    ((ISearchEngine)menuItem?.Tag)?.SearchText(str);
                }
            }
        }

        private async void EditCover_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = this.ViewModel.InfoView;

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

            var dlg = new EntityEditorWindow()
            {
                Owner = w
            }.CreateOrCloneMode(this.ViewModel.Video.Source);

            if (dlg.ShowDialog() == true)
            {
                await this.ViewModel.ReloadVideoAsync();
                this.ViewModel.EntitesView.View.Refresh();
            }
        }

        private void EditVideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.InfoView.OpenEditorWindows(this.TryFindParent<Window>()))
            {
                this.ViewModel.ReloadEpisodes();
                this.ViewModel.Background?.BeginUpdateCover();
            }
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

                var dlg = new EntityEditorWindow()
                {
                    Owner = w
                }.ModifyMode(this.ViewModel.Video.Source, vm.Source);

                if (dlg.ShowDialog() == true)
                {
                    vm.RefreshProperties();
                }
            }
        }

        private void GotoDoubanButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.InfoView.NavigateToDouban();
        }

        private void GotoImdbButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.InfoView.NavigateToImdb();
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
                    await this.ViewModel.ReloadVideoAsync();
                    this.ViewModel.EntitesView.View.Refresh();
                }
            }
        }

        private void CopyStringMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (menuItem?.ItemsSource != null && ReferenceEquals(menuItem, e.OriginalSource))
            {
                return;
            }

            var osItem = e.OriginalSource as MenuItem;

            if (osItem != null)
            {
                var str = osItem.DataContext as string ?? osItem.Header as string;

                if (str != null)
                {
                    Clipboard.SetText(str);
                }
            }
        }

        private void EditSeriesMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var seriesViewModel = this.ViewModel.InfoView.SeriesView;
            if (seriesViewModel.OpenEditorWindows(this.TryFindParent<Window>()))
            {
                this.ViewModel.Background?.BeginUpdateCover();
            }
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1 && this.ViewModel.InfoView.Cover != null)
            {
                var buffer = this.ViewModel.InfoView.Cover.BinaryData;
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

        private async void CloneEntityButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as EntityViewModel;

            if (vm != null)
            {
                var w = this.TryFindParent<MetroWindow>();

                var dlg = new EntityEditorWindow()
                {
                    Owner = w
                }.CreateOrCloneMode(this.ViewModel.Video.Source, vm.Source);

                if (dlg.ShowDialog() == true)
                {
                    await this.ViewModel.ReloadVideoAsync();
                    this.ViewModel.EntitesView.View.Refresh();
                }
            }
        }

        private void WatchedsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = this.WatchedsListView.SelectedItem;
            if (selected != null)
            {
                this.WatchedsListView.SelectedItem = null;
                var box = (VideoViewerViewModel.WatchedEpisodeChecker)selected;
                box.SetIsWatchedAndNotify(!box.IsWatched);
            }
        }
    }
}
