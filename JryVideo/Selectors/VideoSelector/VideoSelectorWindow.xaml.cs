using JryVideo.Common;
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

        public static SelectResult<JryVideoInfo> Select(Window parent, SeriesViewModel source, JryVideoInfo without = null, string defaultId = null)
        {
            var dialog = new VideoSelectorWindow() { Owner = parent };
            if (without != null)
            {
                dialog.SelectVideoViewModel.Withouts.Add(without.Id);
            }
            dialog.SelectVideoViewModel.SetSeries(source, defaultId);

            return dialog.ShowDialog() == true
                ? SelectResult<JryVideoInfo>.Selected(dialog.SelectVideoViewModel.Items.Selected?.Source)
                : SelectResult<JryVideoInfo>.NonAccept;
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
            => this.DialogResult = true;

        private void RemoveSelectButton_OnClick(object sender, RoutedEventArgs e)
            => this.SelectVideoViewModel.Items.Selected = null;
    }
}
