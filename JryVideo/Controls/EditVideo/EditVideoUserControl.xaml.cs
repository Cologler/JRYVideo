using JryVideo.Common;
using JryVideo.Common.Dialogs;
using JryVideo.Editors.CoverEditor;
using JryVideo.Managers.FlagManager;
using JryVideo.Selectors.VideoSelector;
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
            CoverEditorWindow dlg;
            if (this.ViewModel.Cover != null)
            {
                dlg = new CoverEditorWindow(this.ViewModel.Cover);
            }
            else
            {
                dlg = new CoverEditorWindow();
                if (this.ViewModel.Action == ObjectChangedAction.Create /* create a video */ ||
                    this.ViewModel.Source.CoverId == null /* video has not cover */)
                {
                    dlg.ViewModel.CreateMode();
                }
                else
                {
                    var cover = await this.ViewModel.GetManagers().CoverManager.FindAsync(this.ViewModel.Source.CoverId);
                    if (cover == null) // database error ?
                    {
                        this.ShowJryVideoMessage("error", "get cover fail");
                        return;
                    }
                    else
                    {
                        dlg.ViewModel.ModifyMode(cover);
                    }
                }
            }

            if (dlg.ViewModel.DoubanId.IsNullOrWhiteSpace() && !this.ViewModel.DoubanId.IsNullOrWhiteSpace())
            {
                dlg.ViewModel.DoubanId = this.ViewModel.DoubanId;
            }

            if (dlg.ViewModel.ImdbId.IsNullOrWhiteSpace())
            {
                if (!this.ViewModel.Parent.Source.ImdbId.IsNullOrWhiteSpace())
                {
                    dlg.ViewModel.ImdbId = this.ViewModel.Parent.Source.ImdbId;
                }
                else if (!this.ViewModel.ImdbId.IsNullOrWhiteSpace())
                {
                    dlg.ViewModel.ImdbId = this.ViewModel.ImdbId;
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

        private void LoadDoubanButton_OnClick(object sender, RoutedEventArgs e) => this.ViewModel.LoadFromDouban();

        private void SelectLastVideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = VideoSelectorWindow.Select(this.TryFindParent<Window>(),
                this.ViewModel.Parent,
                without: this.ViewModel.Source,
                defaultId: this.ViewModel.LastVideoViewModel?.Source.Id);
            if (result.IsAccept)
            {
                this.ViewModel.ChangeContextVideo(true, result.Value);
            }
        }

        private void SelectNextVideoButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = VideoSelectorWindow.Select(this.TryFindParent<Window>(),
                this.ViewModel.Parent,
                without: this.ViewModel.Source,
                defaultId: this.ViewModel.NextVideoViewModel?.Source.Id);
            if (result.IsAccept)
            {
                this.ViewModel.ChangeContextVideo(false, result.Value);
            }
        }
    }
}
