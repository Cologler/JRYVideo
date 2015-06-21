using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Common;

namespace JryVideo.Main
{
    public class MainViewModel : JasilyViewModel
    {
        public MainViewModel()
        {
            this.VideosObservableCollection = new ObservableCollection<VideoViewModel>();
            this.VideosView = new ListCollectionView(this.VideosObservableCollection);
            this.VideosView.Filter = new Predicate<object>(this.SearchFilter);
        }

        private bool SearchFilter(object obj)
        {
            return true;
        }

        private ObservableCollection<VideoViewModel> VideosObservableCollection;

        public ListCollectionView VideosView { get; private set; }

        public async Task LoadAsync()
        {
            var manager = JryVideo.Core.JryVideoCore.Current.CurrentDataCenter.SeriesManager;

            var series = await manager.LoadAsync();

            this.VideosObservableCollection.AddRange(
                await Task.Run(() => series.SelectMany(VideoViewModel.Create)));
        }
    }
}