using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Add.SelectSeries;
using JryVideo.Core;

namespace JryVideo.Add.SeriesSelector
{
    public sealed class SeriesSelectorViewModel : JasilyViewModel
    {
        private string filterText;

        public SeriesSelectorViewModel()
        {
            this.Items = new JasilyCollectionView<SeriesViewModel>
            {
                Filter = this.OnFilter
            };
        }

        public JasilyCollectionView<SeriesViewModel> Items { get; private set; }

        private bool OnFilter(SeriesViewModel obj)
        {
            if (String.IsNullOrWhiteSpace(this.FilterText)) return true;

            if (obj == null) return true;

            var keyword = this.FilterText.Trim().ToLower();

            return obj.Source.Names.Any(z => z.ToLower().Contains(keyword));
        }

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

        public async Task LoadAsync()
        {
            var seriesManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager;

            this.Items.Collection.AddRange(
                await Task.Run(async () =>
                    (await seriesManager.LoadAsync())
                    .Select(z => new SeriesViewModel(z))));
        }
    }
}