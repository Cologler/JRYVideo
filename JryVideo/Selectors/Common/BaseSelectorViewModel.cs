using System.Collections.Generic;
using Jasily.ComponentModel;
using Jasily.Windows.Data;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Selectors.Common
{
    public abstract class BaseSelectorViewModel<TViewModel, TEntity> : JasilyViewModel
        where TViewModel : JasilyViewModel<TEntity>
        where TEntity : JryObject
    {
        private string filterText;
        private TEntity without;

        public BaseSelectorViewModel()
        {
            this.Items = new JasilyCollectionView<TViewModel>
            {
                Filter = this.OnFilter
            };
        }

        public JasilyCollectionView<TViewModel> Items { get; private set; }

        public TEntity Without
        {
            get { return this.without; }
            set
            {
                if (this.without != value)
                {
                    this.without = value;
                    this.LazyFilter();
                }
            }
        }

        public HashSet<string> Withouts { get; } = new HashSet<string>();

        protected virtual bool OnFilter(TViewModel obj) => this.without?.Id != obj.Source.Id && !this.Withouts.Contains(obj.Source.Id);

        public string FilterText
        {
            get { return this.filterText; }
            set
            {
                if (this.SetPropertyRef(ref this.filterText, value))
                {
                    this.LazyFilter();
                }
            }
        }

        private async void LazyFilter()
        {
            var text = this.FilterText;

            await Task.Delay(400);

            if (text == this.FilterText)
            {
                this.OnResetFilter(text);
                this.Items.View.Refresh();
            }
        }

        protected virtual void OnResetFilter(string filterText)
        {

        }
    }
}