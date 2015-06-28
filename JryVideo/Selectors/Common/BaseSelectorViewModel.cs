using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JryVideo.Selectors.Common
{
    public abstract class BaseSelectorViewModel<T> : JasilyViewModel
    {
        private string filterText;

        public BaseSelectorViewModel()
        {
            this.Items = new JasilyCollectionView<T>
            {
                Filter = this.OnFilter
            };
        }

        public JasilyCollectionView<T> Items { get; private set; }

        protected abstract bool OnFilter(T obj);

        public string FilterText
        {
            get { return this.filterText; }
            set
            {
                if (this.SetPropertyRef(ref this.filterText, value))
                    this.LazyFilter();
            }
        }

        private async void LazyFilter()
        {
            var text = this.FilterText;

            await Task.Delay(400);

            if (text == this.FilterText)
            {
                this.Items.View.Refresh();
            }
        }
    }
}