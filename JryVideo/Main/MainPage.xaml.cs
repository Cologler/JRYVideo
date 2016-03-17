using Jasily.Desktop.Windows.Navigation;
using JryVideo.Add;
using JryVideo.Common;
using JryVideo.Common.Dialogs;
using JryVideo.Core;
using JryVideo.Data;
using JryVideo.Editors.CoverEditor;
using JryVideo.Editors.PasswordEditor;
using JryVideo.Selectors.SeriesSelector;
using JryVideo.Selectors.VideoSelector;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JryVideo.Main
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : Page
    {
        public event EventHandler<VideoInfoViewModel> VideoSelected;

        private MainViewModel ViewModel;
        private ProcessTrackTask processTrackTask;

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.FrameworkElement.Initialized"/> 事件。 每当在内部将 <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> 设置为 true 时，都将调用此方法。
        /// </summary>
        /// <param name="e">包含事件数据的 <see cref="T:System.Windows.RoutedEventArgs"/>。</param>
        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                await JryVideoCore.Current.InitializeAsync();
                this.DataContext = this.ViewModel = new MainViewModel();
                this.ViewModel.ReloadAsync();
                this.processTrackTask = new ProcessTrackTask(this.ViewModel);
                this.processTrackTask.CurrentWatchVideo += this.ProcessTrackTask_CurrentWatchVideo;
            }
        }

        private void ProcessTrackTask_CurrentWatchVideo(object sender, VideoInfoViewModel e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                var win = this.TryFindParent<MainWindow>();
                if (win == null) return;
                if (ReferenceEquals(win.MainFrame.Content, this) &&
                    win.MainFrameNavigationStatus.Status != NavigationServiceStatus.Navigating)
                {
                    if (this.ViewModel.VideosViewModel.VideosView.Collection.Contains(e))
                    {
                        this.VideoSelected?.Invoke(this, e);
                    }
                }
            });
        }

        private async void EditCover_OnClick(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var vm = frameworkElement.DataContext as VideoInfoViewModel;
            if (vm == null) return;

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

        private void AddMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var adder = new AddWindow()
            {
                Owner = this.TryFindParent<Window>()
            };
            if (adder.ShowDialog() == true)
            {
                if (this.ViewModel.VideosViewModel.IsOnlyTracking == adder.DialogResultObject.IsTracking)
                {
                    this.ViewModel.ReloadAsync();
                }
            }
        }

        private void VideoPanel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm != null) this.VideoSelected?.Invoke(this, vm);
        }

        private async void SearchTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ViewModel.VideosViewModel.IsOnlyTracking = false;
                this.ViewModel.VideosViewModel.FilterText = string.Empty;
                await this.ViewModel.VideosViewModel.ReloadAsync();
            }
        }

        private async void LastPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.LastPageAsync();
            // ReSharper disable once AssignNullToNotNullAttribute
            this.VideosListView.ScrollIntoView(this.VideosListView.ItemsSource.OfType<object>().FirstOrDefault());
        }

        private async void NextPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.NextPageAsync();
            // ReSharper disable once AssignNullToNotNullAttribute
            this.VideosListView.ScrollIntoView(this.VideosListView.ItemsSource.OfType<object>().FirstOrDefault());
        }

        private void RefreshGroupStyle()
        {
            this.VideosListView.GroupStyle.Clear();
            if (this.ViewModel.VideosViewModel.IsOnlyTracking)
            {
                this.VideosListView.GroupStyle.Add(this.Resources["TrackingGroupStyle"] as GroupStyle);
            }
        }

        private async void IsOnlyTrackingCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.VideosViewModel.FilterText = string.Empty;
            await this.ViewModel.VideosViewModel.ReloadAsync();
            this.RefreshGroupStyle();
        }

        private async void IsOnlyTrackingCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.VideosViewModel.FilterText = string.Empty;
            await this.ViewModel.VideosViewModel.ReloadAsync();
            this.RefreshGroupStyle();
        }

        public void RefreshVideo(VideoInfoViewModel vm)
        {
            this.ViewModel.VideosViewModel.VideosView.Collection.Remove(vm);
            this.ViewModel.VideosViewModel.RefreshVideo(vm);
            this.ViewModel.VideosViewModel.VideosView.Collection.Add(vm);
        }

        private async void AllAiredMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm == null) return;
            if (await vm.AllAiredAsync())
            {
                this.RefreshVideo(vm);
            }
        }

        private async void TrackMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm == null) return;
            await vm.TrackAsync();
        }

        private async void UntrackMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm == null) return;
            if (await vm.UntrackAsync() && this.ViewModel.VideosViewModel.IsOnlyTracking)
            {
                this.ViewModel.VideosViewModel.VideosView.Collection.Remove(vm);
            }
        }

        public void Refresh()
        {
            if (this.ViewModel == null) return;
            this.ViewModel.VideosViewModel.RefreshAll();
            this.ViewModel.VideosViewModel.VideosView.View.Refresh();
        }

        private void FilterSeries_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm == null) return;
            this.ViewModel.VideosViewModel.FilterText = vm.SeriesView.Source.Id;
        }

        private async void ModeSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = this.ViewModel.SelectedMode;

            if (selected != null)
            {
                if (selected.Source == JryVideoDataSourceProviderManagerMode.Public)
                {
                    JryVideoCore.Current.Switch(JryVideoDataSourceProviderManagerMode.Public);
                    await this.ViewModel.VideosViewModel.ReloadAsync();
                }
                else
                {
                    var secure = JryVideoCore.Current.SecureDataCenter;

                    if (!secure.IsWork)
                    {
                        this.ShowJryVideoMessage("info", "current database not support private mode.");
                        this.SwitchPublic();
                        return;
                    }

                    string password = null;

                    if (!await secure.ProviderManager.HasPasswordAsync())
                    {
                        var dlg = new PasswordEditorWindow();
                        dlg.MessageTextBlock.Text = "first time you must set a password.";
                        dlg.MessageTextBlock.Visibility = Visibility.Visible;
                        dlg.Owner = this.TryFindParent<Window>();
                        if (dlg.ShowDialog() != true)
                        {
                            this.SwitchPublic();
                            return;
                        }
                        password = dlg.PasswordResult;
                    }
                    else
                    {
                        var pwDlg = new PasswordWindow();
                        pwDlg.Owner = this.TryFindParent<Window>();
                        if (pwDlg.ShowDialog() == true && !pwDlg.PasswordBox.Password.IsNullOrWhiteSpace())
                        {
                            password = pwDlg.PasswordBox.Password;
                        }
                    }

                    if (!password.IsNullOrWhiteSpace())
                    {
                        var hash = JasilyHash.Create(HashType.SHA1).ComputeHashString(password);
                        if (await secure.ProviderManager.PasswordAsync(hash))
                        {
                            JryVideoCore.Current.Switch(JryVideoDataSourceProviderManagerMode.Private);
                            await this.ViewModel.VideosViewModel.ReloadAsync();
                            return;
                        }
                    }

                    this.ShowJryVideoMessage("error", "password error.");
                    this.SwitchPublic();
                }
            }
        }

        private void SwitchPublic()
        {
            this.ViewModel.SelectedMode = this.ViewModel.ModeCollection.First(
                z => z.Source == JryVideoDataSourceProviderManagerMode.Public);
        }

        private void WatchedEpisode_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            vm?.Watch();
            this.ViewModel.VideosViewModel.VideosView.Collection.Remove(vm);
            this.ViewModel.VideosViewModel.VideosView.Collection.Add(vm);
        }

        private void EditVideoInfo_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm?.OpenEditorWindows(this.TryFindParent<Window>()) == true)
            {
                this.RefreshVideo(vm);
            }
        }

        private void EditSeries_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm?.SeriesView?.OpenEditorWindows(this.TryFindParent<Window>()) == true)
            {
                this.RefreshVideo(vm);
            }
        }

        private async void CombineVideo_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm != null)
            {
                var result = VideoSelectorWindow.Select(this.TryFindParent<Window>(),
                    vm.SeriesView, without: vm);
                if (!result.IsAccept) return;
                var video = result.Value;
                if (video == null) return;
                var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager.GetVideoInfoManager(vm.SeriesView);
                var can = await JryVideoCore.Current.CurrentDataCenter.CanCombineAsync(manager, vm, video);
                if (!can.CanCombine)
                {
                    this.ShowJryVideoMessage("can not combine", can.Message);
                }
                else
                {
                    this.CombineConfirm(async () =>
                    {
                        await JryVideoCore.Current.CurrentDataCenter.CombineAsync(manager, vm, video);
                    });
                }
            }
        }

        private async void CombineSeries_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm != null)
            {
                var series = SeriesSelectorWindow.Select(this.TryFindParent<Window>(),
                    without: vm.SeriesView);
                if (series == null) return;
                var can = await JryVideoCore.Current.CurrentDataCenter.CanCombineAsync(vm.SeriesView, series);
                if (!can.CanCombine)
                {
                    this.ShowJryVideoMessage("can not combine", can.Message);
                }
                else
                {
                    this.CombineConfirm(async () =>
                    {
                        await JryVideoCore.Current.CurrentDataCenter.CombineAsync(vm.SeriesView, series);
                    });
                }
            }
        }

        private async void CombineConfirm(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (await this.TryFindParent<MetroWindow>().ShowMessageAsync(
                "warnning", "are you sure you want to combine this two?",
                MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative)
            {
                action();
                await this.ViewModel.VideosViewModel.ReloadAsync();
            }
        }
    }
}
