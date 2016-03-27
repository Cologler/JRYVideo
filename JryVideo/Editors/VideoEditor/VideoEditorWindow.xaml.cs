using JryVideo.Common;
using JryVideo.Model;
using MahApps.Metro.Controls;
using System.Windows;

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

        public VideoEditorWindow(VideoInfoViewModel video)
            : this()
        {
            this.EditVideoUserControl.ViewModel.Updated += this.VideoInfo_Updated;
            this.EditVideoUserControl.ViewModel.Parent = video.SeriesView;
            this.EditVideoUserControl.ViewModel.ModifyMode(video);
        }

        void VideoInfo_Updated(object sender, JryVideoInfo e)
        {
            if (this.GetUIDispatcher().CheckAccessOrBeginInvoke(this.VideoInfo_Updated, sender, e))
            {
                this.DialogResult = true;
            }
        }


    }
}
