using JryVideo.Add;
using JryVideo.Common;
using JryVideo.Common.Dialogs;
using JryVideo.Core;
using JryVideo.Data;
using JryVideo.Editors.CoverEditor;
using JryVideo.Editors.PasswordEditor;
using JryVideo.Model;
using MahApps.Metro.Controls;
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
                this.ViewModel.LoadAsync();
            }
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
            if (new AddWindow()
            {
                Owner = this.TryFindParent<Window>()
            }.ShowDialog() == true)
            {
                this.ViewModel.LoadAsync();
            }
        }

        private void VideoPanel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var vm = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            if (vm != null) this.VideoSelected.Fire(this, vm);
        }

        private async void SearchTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ViewModel.VideosViewModel.IsOnlyTracking = false;
                await this.ViewModel.VideosViewModel.RefreshAsync();
            }
        }

        private async void LastPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.LastPageAsync();
            this.VideosListView.ScrollIntoView(this.VideosListView.ItemsSource.OfType<object>().FirstOrDefault());
        }

        private async void NextPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.NextPageAsync();
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
            await this.ViewModel.SetOnlyTrackingAsync();
            this.RefreshGroupStyle();
        }

        private async void IsOnlyTrackingCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.UnsetOnlyTrackingAsync();
            this.RefreshGroupStyle();
        }

        public void RefreshVideo(VideoInfoViewModel vm)
        {
            this.ViewModel.VideosViewModel.VideosView.Collection.Remove(vm);
            vm.RefreshProperties();
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
            foreach (var viewModel in this.ViewModel.VideosViewModel.VideosView.Collection)
            {
                viewModel.RefreshProperties();
            }
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
                    await this.ViewModel.VideosViewModel.RefreshAsync();
                }
                else
                {
                    var pw = await JryVideoCore.Current.SecureDataCenter.ProviderManager.GetSettingSet()
                        .FindAsync("password_sha1");

                    if (pw == null)
                    {
                        var dlg = new PasswordEditorWindow();
                        dlg.MessageTextBlock.Text = "first time you must set a password.";
                        dlg.MessageTextBlock.Visibility = Visibility.Visible;
                        dlg.Owner = this.TryFindParent<Window>();
                        if (dlg.ShowDialog() != true)
                        {
                            this.ViewModel.SelectedMode = this.ViewModel.ModeCollection.First(
                                z => z.Source == JryVideoDataSourceProviderManagerMode.Public);
                            return;
                        }
                        var hash = JasilyHash.Create(HashType.SHA1).ComputeHashString(dlg.PasswordResult);
                        pw = new JrySettingItem("password_sha1", hash);
                        await JryVideoCore.Current.SecureDataCenter.ProviderManager.GetSettingSet().InsertAsync(pw);
                    }

                    var pwDlg = new PasswordWindow();
                    pwDlg.Owner = this.TryFindParent<Window>();
                    if (pwDlg.ShowDialog() == true)
                    {
                        if (pwDlg.PasswordBox.Password.IsNullOrWhiteSpace() ||
                            JasilyHash.Create(HashType.SHA1).ComputeHashString(pwDlg.PasswordBox.Password) != pw.Value)
                        {
                            this.ShowJryVideoMessage("error", "password error.");
                        }
                        else
                        {
                            JryVideoCore.Current.Switch(JryVideoDataSourceProviderManagerMode.Private);
                            await this.ViewModel.VideosViewModel.RefreshAsync();
                            return;
                        }
                    }

                    this.ViewModel.SelectedMode = this.ViewModel.ModeCollection.First(
                        z => z.Source == JryVideoDataSourceProviderManagerMode.Public);
                }
            }
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
    }
}
