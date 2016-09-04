using System;
using System.Windows;
using JryVideo.Model;
using MahApps.Metro.Controls;

namespace JryVideo.Editors.EntityEditor
{
    /// <summary>
    /// EntityEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EntityEditorWindow : MetroWindow
    {
        public EntityEditorPage Page { get; private set; } = new EntityEditorPage();

        public EntityEditorWindow()
        {
            this.InitializeComponent();
        }

        public EntityEditorWindow CreateOrCloneMode(Model.JryVideoInfo video, Resource entity = null)
        {
            this.Page.ViewModel.Initialize(video);
            if (entity == null)
            {
                this.Page.ViewModel.CreateMode();
            }
            else
            {
                this.Page.ViewModel.CloneMode(entity);
            }
            this.TitleTextBlock.Text = "creator";
            return this;
        }

        public EntityEditorWindow ModifyMode(Model.JryVideoInfo video, Resource entity)
        {
            this.Page.ViewModel.Initialize(video);
            this.Page.ViewModel.ModifyMode(entity);
            this.TitleTextBlock.Text = "editor";
            return this;
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Window.SourceInitialized"/> 事件。
        /// </summary>
        /// <param name="e">一个 <see cref="T:System.EventArgs"/>，其中包含事件数据。</param>
        protected override async void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.MainFrame.Navigate(this.Page);

            await this.Page.ViewModel.LoadAsync();
        }

        private async void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await this.Page.ViewModel.CommitAsync(this) != null)
            {
                this.DialogResult = true;
            }
        }
    }
}
