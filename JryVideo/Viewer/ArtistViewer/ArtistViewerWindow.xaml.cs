using System.Windows;

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

        private async void EditArtistButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.ArtistView.EditAsync(this);
        }
    }
}
