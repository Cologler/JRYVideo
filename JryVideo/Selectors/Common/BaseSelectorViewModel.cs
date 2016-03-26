using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasily.Diagnostics;

namespace JryVideo.Selectors.Common
{
    public abstract class BaseSelectorViewModel<TViewModel, TEntity> : JasilyViewModel
        where TViewModel : JasilyViewModel<TEntity>
        where TEntity : JryObject
    {
        private string filterText;

        protected BaseSelectorViewModel()
        {
            this.Items = new JasilyCollectionView<TViewModel>
            {
                Filter = this.OnFilter
            };
        }

        public JasilyCollectionView<TViewModel> Items { get; }

        public HashSet<string> Withouts { get; } = new HashSet<string>();

        protected virtual bool OnFilter(TViewModel obj) => !this.Withouts.Contains(obj.Source.Id);

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
                JasilyDebug.Pointer();
                this.Items.View.Refresh();
                JasilyDebug.Pointer();
            }
        }

        protected virtual void OnResetFilter(string filterText)
        {

        }
    }
}