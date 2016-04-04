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
        public event EventHandler ClickedGoBack;
        public event EventHandler<VideoInfoViewModel> ClickedOtherVideo;

        public VideoViewerPage()
        {
            this.InitializeComponent();

            this.GoBackButton.Click += this.GoBackButton_Click;
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e) => this.ClickedGoBack?.Invoke(this);

        public VideoViewerViewModel ViewModel { get; private set; }

        internal static VideoViewerPage BuildPage(VideoInfoViewModel info)
        {
            var vm = new VideoViewerViewModel(info);

            var page = new VideoViewerPage()
            {
                ViewModel = vm,
                DataContext = vm
            };

            SearchEngineCenter.Engines.ForEach(z => AppendToContextMenu(page.SeriesContextMenu, z));
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

            if (dlg.ShowDialog() == true || this.ViewModel.Video.IsObsolete)
            {
                await this.ViewModel.ReloadVideoAsync();
            }
        }

        private void EditVideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.InfoView.OpenEditorWindows(this.TryFindParent<Window>()))
            {
                this.ViewModel.ReloadEpisodes();
                this.ViewModel.Background.BeginUpdateCoverIfEmpty();
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

        private void DeleteEntityButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.DeleteConfirm(async () =>
            {
                var entity = ((FrameworkElement)sender).DataContext as EntityViewModel;
                if (entity != null)
                {
                    await this.ViewModel.Video.RemoveAsync(entity);
                    await this.ViewModel.ReloadVideoAsync();
                    this.ViewModel.EntitesView.View.Refresh();
                }
            });
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
                this.ViewModel.Background.BeginUpdateCoverIfEmpty();
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

        private void ShowActorMenuItem_OnClick(object sender, RoutedEventArgs e)
            => this.ActorFlyout.IsOpen = !this.ActorFlyout.IsOpen;

        private void ResetBackgroundMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Background.Reset();
        }

        private async void SelectBackgroundMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.Background.StartSelect(this.TryFindParent<Window>());
        }

        private void ActorEditMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoRoleViewModel;
            vm?.BeginEdit(this.TryFindParent<Window>());
        }

        private void ActorMoveToAnotherMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoRoleViewModel;
            vm?.BegionMoveToAnotherCollection();
        }

        private void ActorDeleteMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            this.DeleteConfirm(async () =>
            {
                var vm = ((FrameworkElement)sender).DataContext as VideoRoleViewModel;
                if (vm != null) await this.ViewModel.VideoRoleCollection.DeleteAsync(vm);
            });
        }

        private async void DeleteConfirm(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (await this.TryFindParent<MetroWindow>().ShowMessageAsync(
                "warnning", "are you sure you want to delete it?",
                MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
            {
                action();
            }
        }

        private void ActorViewMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoRoleViewModel;
            if (vm != null)
            {
                vm.ShowActor(this.TryFindParent<Window>());
                vm.RefreshProperties();
            }
        }

        private void GoLastVideoMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var last = this.ViewModel.InfoView.TryFindLastViewModel();
            if (last != null) this.ClickedOtherVideo?.Invoke(this, last);
        }

        private void GoNextVideoMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var next = this.ViewModel.InfoView.TryFindNextViewModel();
            if (next != null) this.ClickedOtherVideo?.Invoke(this, next);
        }

        private void WatchAllMenuItem_OnClick(object sender, RoutedEventArgs e) => this.ViewModel.WatchAll();

        private void WatchReverseMenuItem_OnClick(object sender, RoutedEventArgs e) => this.ViewModel.WatchReverse();

        private void WatchNoneMenuItem_OnClick(object sender, RoutedEventArgs e) => this.ViewModel.WatchNone();

        private async void WatchSaveMenuItem_OnClick(object sender, RoutedEventArgs e) => await this.ViewModel.WatchSaveAsync();

        private async void CombineActorsMenuItem_OnClick(object sender, RoutedEventArgs e)
            => await this.ViewModel.VideoRoleCollection.CombineActorsAsync();
    }
}
