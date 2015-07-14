using System.Windows.Controls;

namespace JryVideo.Selectors.ArtistSelector
{
    /// <summary>
    /// ArtistSelectorPage.xaml 的交互逻辑
    /// </summary>
    public partial class ArtistSelectorPage : Page
    {
        public ArtistSelectorViewModel ViewModel { get; private set; }

        public ArtistSelectorPage()
        {
            this.InitializeComponent();
        }
    }
}
