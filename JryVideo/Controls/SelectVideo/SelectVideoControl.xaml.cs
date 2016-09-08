using System;
using System.Windows;
using System.Windows.Controls;
using Jasily.Diagnostics;
using JryVideo.Common;

namespace JryVideo.Controls.SelectVideo
{
    /// <summary>
    /// SelectVideoControl.xaml 的交互逻辑
    /// </summary>
    public partial class SelectVideoControl
    {
        public event EventHandler OnCommited;

        public bool IsCommited { get; private set; }

        public SelectVideoControl()
        {
            this.DataContext = this.ViewModel;
            this.InitializeComponent();
            this.VideosListView.GroupStyle.Add(this.Resources["GroupIndexGroupStyle"] as GroupStyle);
        }

        public SelectVideoViewModel ViewModel { get; } = new SelectVideoViewModel();

        public void Initialize(SeriesViewModel series, string defaultId = null)
        {
            this.ViewModel.Series = series;
            this.ViewModel.DefaultVideoId = defaultId;
            this.ViewModel.Initialize();
        }

        private async void MoveToGroupMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var viewModel = ((FrameworkElement)sender).DataContext as VideoInfoViewModel;
            var group = ((FrameworkElement)e.OriginalSource).DataContext as SelectVideoViewModel.Group;
            JasilyDebug.AssertNotNull(viewModel);
            JasilyDebug.AssertNotNull(group);
            if (this.ViewModel.CanMoveTo(viewModel, group))
            {
                await this.ViewModel.MoveToAsync(viewModel, group);
                this.IsCommited = true;
                this.OnCommited?.Invoke(this);
            }
        }
    }
}
