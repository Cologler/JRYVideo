using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.Common;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace JryVideo.Selectors.ArtistSelector
{
    public sealed class ArtistSelectorViewModel : BaseSelectorViewModel<ArtistViewModel, JryArtist>
    {
        public ArtistSelectorViewModel()
        {
            this.SelectedItems = new ObservableCollection<ArtistViewModel>();
            this.SelectedItems.CollectionChanged += this.SelectedItems_CollectionChanged;
        }

        void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    this.Items.View.Refresh();
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ObservableCollection<ArtistViewModel> SelectedItems { get; private set; }

        protected override bool OnFilter(ArtistViewModel obj)
        {
            return base.OnFilter(obj) && this.FilterBySelected(obj) && this.FilterByKeyword(obj);
        }

        private bool FilterBySelected(ArtistViewModel obj)
        {
            return !this.SelectedItems.Contains(obj);
        }

        private bool FilterByKeyword(ArtistViewModel obj)
        {
            if (String.IsNullOrWhiteSpace(this.FilterText)) return true;

            if (obj == null) return true;

            var keyword = this.FilterText.Trim().ToLower();

            return obj.Source.Names.Any(z => z.ToLower().Contains(keyword));
        }
    }
}