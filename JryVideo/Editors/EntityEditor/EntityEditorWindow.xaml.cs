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
        public EntityEditorPage Page { get; private set; }

        public EntityEditorWindow()
        {
            this.InitializeComponent();
        }

        public EntityEditorWindow(Model.JryVideo video)
            : this()
        {
            this.Page = new EntityEditorPage(video);
            this.Page.ViewModel.CreateMode();
            this.TitleTextBlock.Text = "creator";
        }

        public EntityEditorWindow(Model.JryVideo video, JryEntity entity)
            : this(video)
        {
            this.Page.ViewModel.ModifyMode(entity);
            this.TitleTextBlock.Text = "editor";
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Window.SourceInitialized"/> 事件。
        /// </summary>
        /// <param name="e">一个 <see cref="T:System.EventArgs"/>，其中包含事件数据。</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.MainFrame.Navigate(this.Page);
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
