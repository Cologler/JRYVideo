using System.Linq;
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

        public static VideoSelectorWindow Create(SeriesViewModel source)
        {
            var dialog = new VideoSelectorWindow
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(z => z.IsActive)
            };
            dialog.SelectVideoControl.ViewModel.Series = source;
            return dialog;
        }

        public SelectResult<JryVideoInfo> GetResult()
        {
            this.SelectVideoControl.ViewModel.Initialize();
            return this.ShowDialog() == true
                ? SelectResult<JryVideoInfo>.Selected(this.SelectVideoControl.ViewModel.Items.Selected?.Source)
                : SelectResult<JryVideoInfo>.NonAccept;
        }

        public string DefaultVideoId
        {
            get { return this.SelectVideoControl.ViewModel.DefaultVideoId; }
            set { this.SelectVideoControl.ViewModel.DefaultVideoId = value; }
        }

        public void AddWithout(string videoId)
        {
            if (videoId == null) return;
            this.SelectVideoControl.ViewModel.Withouts.Add(videoId);
        }

        public static SelectResult<JryVideoInfo> Select(SeriesViewModel source, params JryVideoInfo[] withouts)
        {
            var dialog = Create(source);
            foreach (var id in withouts.Select(z => z.Id))
            {
                dialog.AddWithout(id);
            }
            return dialog.GetResult();
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
            => this.DialogResult = true;

        private void RemoveSelectButton_OnClick(object sender, RoutedEventArgs e)
            => this.SelectVideoControl.ViewModel.Items.Selected = null;
    }
}
