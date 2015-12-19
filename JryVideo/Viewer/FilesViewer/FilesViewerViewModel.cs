using Jasily.ComponentModel;
using Jasily.Windows.Data;

namespace JryVideo.Viewer.FilesViewer
{
    public sealed class FilesViewerViewModel : JasilyViewModel
    {
        private bool empty = true;

        public FilesViewerViewModel()
        {
            this.FilesView = new JasilyCollectionView<FileItemViewModel>()
            {
                Filter = this.ItemFilter
            };
            this.FilesView.Collection.CollectionChanged += this.Collection_CollectionChanged;
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Empty = this.FilesView.Collection.Count == 0;
        }

        public bool Empty
        {
            get { return this.empty; }
            set { this.SetPropertyRef(ref this.empty, value); }
        }

        private bool ItemFilter(FileItemViewModel obj)
        {
            return true;
        }

        public JasilyCollectionView<FileItemViewModel> FilesView { get; private set; }
    }
}