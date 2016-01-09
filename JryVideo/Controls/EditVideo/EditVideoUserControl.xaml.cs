using JryVideo.Core;
using JryVideo.Editors.CoverEditor;
using JryVideo.Managers.FlagManager;
using JryVideo.Selectors.ArtistSelector;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Enums;
using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Controls.EditVideo
{
    /// <summary>
    /// EditVideoUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class EditVideoUserControl : UserControl
    {
        public EditVideoViewModel ViewModel { get; private set; }

        public EditVideoUserControl()
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
                this.DataContext = this.ViewModel = new EditVideoViewModel();
                await this.ViewModel.LoadAsync();
            }
        }

        private async void EditCoverButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new CoverEditorWindow();

            if (this.ViewModel.Cover != null)
            {
                if (this.ViewModel.Cover.Action == ObjectChangedAction.Create)
                {
                    dlg.ViewModel.CreateMode();
                }
                else
                {
                    dlg.ViewModel.ModifyMode(this.ViewModel.Cover.Source);
                }

                dlg.ViewModel.DoubanId = this.ViewModel.Cover.DoubanId;
                dlg.ViewModel.CoverSourceType = this.ViewModel.Cover.CoverSourceType;
                dlg.ViewModel.Uri = this.ViewModel.Cover.Uri;
                dlg.ViewModel.BinaryData = this.ViewModel.Cover.BinaryData;
                dlg.ViewModel.SaveToObject(dlg.ViewModel.Source);
            }
            else
            {
                if (this.ViewModel.Action == ObjectChangedAction.Create /* create a video */ ||
                    this.ViewModel.Source.CoverId == null /* video has not cover */)
                {
                    dlg.ViewModel.CreateMode();
                }
                else
                {
                    var cover = await JryVideoCore.Current.CurrentDataCenter.CoverManager.FindAsync(this.ViewModel.Source.CoverId);

                    if (cover == null) // database error ?
                    {
                        dlg.ViewModel.CreateMode();
                    }
                    else
                    {
                        dlg.ViewModel.ModifyMode(cover);
                    }
                }
            }

            dlg.UpdateRadioButtonCheckedStatus();

            if (dlg.ShowDialog() == true)
            {
                this.ViewModel.Cover = dlg.ViewModel;
            }
        }

        private void EditVideoTypeButton_OnClick(object sender, RoutedEventArgs e)
        {
            new FlagManagerWindow()
            {
                Owner = this.TryFindParent<Window>()
            }.ShowDialog();
        }

        private async void CommitButton_OnClick(object sender, RoutedEventArgs e)
        {
            var window = this.TryFindParent<MetroWindow>();
            await this.ViewModel.CommitAsync(window);
        }

        private void SelectArtistButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new ArtistSelectorWindow();

            if (dlg.ShowDialog() == true)
            {

            }
        }

        private async void LoadDoubanButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.LoadDoubanAsync();
        }
    }
}
