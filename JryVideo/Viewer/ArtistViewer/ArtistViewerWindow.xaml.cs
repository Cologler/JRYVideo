namespace JryVideo.Viewer.ArtistViewer
{
    /// <summary>
    /// ArtistViewerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ArtistViewerWindow
    {
        public ArtistViewerWindow()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        public ArtistViewerViewModel ViewModel { get; } = new ArtistViewerViewModel();
    }
}
