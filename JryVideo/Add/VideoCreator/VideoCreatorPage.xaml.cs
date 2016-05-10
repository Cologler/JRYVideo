using System.Windows;
using System.Windows.Controls;
using JryVideo.Common;
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

        public VideoCreatorPage(SeriesViewModel series)
            : this()
        {
            this.DataContext = this.CreatorViewModel = new VideoCreatorViewModel(series);
            this.EditVideoUserControl.ViewModel.Parent = series;
            this.EditVideoUserControl.ViewModel.Created += this.ViewModel_Created;
        }

        void ViewModel_Created(object sender, JryVideoInfo e)
        {
            if (this.GetUIDispatcher().CheckAccessOrBeginInvoke(this.ViewModel_Created, sender, e))
            {
                var win = this.TryFindParent<AddWindow>();
                win.IsCommited = true;
                win.DialogResultObject = e;
                win.DialogResult = true;
            }
        }
    }
}
