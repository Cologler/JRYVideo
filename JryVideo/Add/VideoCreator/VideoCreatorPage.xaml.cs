using System.Windows;
using System.Windows.Controls;
using JryVideo.Model;
using MahApps.Metro.Controls;

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
            this.EditVideoUserControl.ViewModel.Parent = series;
            this.EditVideoUserControl.ViewModel.Created += this.ViewModel_Created;
        }

        void ViewModel_Created(object sender, JryVideoInfo e)
        {
            if (this.Dispatcher.CheckAccessOrBeginInvoke(this.ViewModel_Created, sender, e))
            {
                this.TryFindParent<Window>().DialogResult = true;
            }
        }
    }
}
