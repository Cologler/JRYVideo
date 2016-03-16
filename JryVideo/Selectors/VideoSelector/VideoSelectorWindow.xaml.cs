using JryVideo.Controls.SelectVideo;
using JryVideo.Model;
using System.Windows;

namespace JryVideo.Selectors.VideoSelector
{
    /// <summary>
    /// VideoSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class VideoSelectorWindow
    {
        public VideoSelectorWindow()
        {
            this.InitializeComponent();
        }

        public SelectVideoViewModel SelectVideoViewModel => this.SelectVideoControl.ViewModel;

        public static SelectResult<JryVideoInfo> Select(Window parent, JrySeries source, JryVideoInfo without = null, string defaultId = null)
        {
            var dialog = new VideoSelectorWindow() { Owner = parent };
            dialog.SelectVideoViewModel.Source = source;
            if (without != null)
            {
                dialog.SelectVideoViewModel.Withouts.Add(without.Id);
            }
            dialog.SelectVideoViewModel.DefaultId = defaultId;
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            dialog.SelectVideoViewModel.RefreshAsync();
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

            return dialog.ShowDialog() == true
                ? SelectResult<JryVideoInfo>.Selected(dialog.SelectVideoViewModel.VideosView.Selected?.Source)
                : SelectResult<JryVideoInfo>.NonAccept;
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
            => this.DialogResult = true;

        private void RemoveSelectButton_OnClick(object sender, RoutedEventArgs e)
            => this.SelectVideoViewModel.VideosView.Selected = null;
    }
}
