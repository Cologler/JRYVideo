using System.Windows.Controls;
using JryVideo.Model;

namespace JryVideo.Add.VideoCreator
{
    /// <summary>
    /// SelectVideoPage.xaml 的交互逻辑
    /// </summary>
    public partial class VideoCreatorPage : Page
    {
        public VideoCreatorViewModel CreatorViewModel { get; private set; }

        public VideoCreatorPage()
        {
            this.InitializeComponent();
        }

        public VideoCreatorPage(JrySeries series)
            : this()
        {
            this.DataContext = this.CreatorViewModel = new VideoCreatorViewModel(series);
        }
    }
}
