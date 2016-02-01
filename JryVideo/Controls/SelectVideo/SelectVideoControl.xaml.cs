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
    }
}
