using System.Windows;
using JryVideo.Model;
using MahApps.Metro.Controls;

namespace JryVideo.Editors.VideoEditor
{
    /// <summary>
    /// VideoEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VideoEditorWindow : MetroWindow
    {
        public VideoEditorWindow()
        {
            this.InitializeComponent();
        }

        public VideoEditorWindow(JrySeries series, JryVideoInfo video)
            : this()
        {
            this.EditVideoUserControl.ViewModel.Updated += this.VideoInfo_Updated;
            this.EditVideoUserControl.ViewModel.Parent = series;
            this.EditVideoUserControl.ViewModel.ModifyMode(video);
        }

        void VideoInfo_Updated(object sender, JryVideoInfo e)
        {
            if (this.Dispatcher.CheckAccessOrBeginInvoke(this.VideoInfo_Updated, sender, e))
            {
                this.DialogResult = true;
            }
        }

        
    }
}
