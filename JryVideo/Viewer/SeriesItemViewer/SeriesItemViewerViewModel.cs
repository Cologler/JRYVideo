using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Common;

namespace JryVideo.Viewer.SeriesItemViewer
{
    public abstract class SeriesItemViewerViewModel : JasilyViewModel
    {
        public SeriesItemViewerViewModel()
        {
            this.VideosObservableCollection = new ObservableCollection<VideoViewModel>();
            this.VideosView = new ListCollectionView(this.VideosObservableCollection);
            this.VideosView.Filter = new Predicate<object>(this.SearchFilter);
        }

        protected virtual bool SearchFilter(object obj)
        {
            return true;
        }

        public ObservableCollection<VideoViewModel> VideosObservableCollection { get; private set; }

        public ListCollectionView VideosView { get; private set; }

        public abstract Task LoadAsync();
    }
}