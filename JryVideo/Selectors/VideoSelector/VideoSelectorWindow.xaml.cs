using System.Windows;
using JryVideo.Common;
using JryVideo.Model;

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

        public void Initialize(SeriesViewModel series, string defaultId = null)
            => this.SelectVideoControl.Initialize(series, defaultId);

        public static SelectResult<JryVideoInfo> Select(Window parent, SeriesViewModel source, JryVideoInfo without = null, string defaultId = null)
        {
            var dialog = new VideoSelectorWindow() { Owner = parent };
            if (without != null)
            {
                dialog.SelectVideoControl.ViewModel.Withouts.Add(without.Id);
            }
            dialog.SelectVideoControl.Initialize(source, defaultId);

            return dialog.ShowDialog() == true
                ? SelectResult<JryVideoInfo>.Selected(dialog.SelectVideoControl.ViewModel.Items.Selected?.Source)
                : SelectResult<JryVideoInfo>.NonAccept;
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
            => this.DialogResult = true;

        private void RemoveSelectButton_OnClick(object sender, RoutedEventArgs e)
            => this.SelectVideoControl.ViewModel.Items.Selected = null;
    }
}
