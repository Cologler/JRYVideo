using JryVideo.Common;

namespace JryVideo.Controls.SelectVideo
{
    /// <summary>
    /// SelectVideoControl.xaml 的交互逻辑
    /// </summary>
    public partial class SelectVideoControl
    {
        public SelectVideoControl()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        public SelectVideoViewModel ViewModel { get; } = new SelectVideoViewModel();

        public void Initialize(SeriesViewModel series, string defaultId = null)
            => this.ViewModel.Initialize(series, defaultId);
    }
}
