using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Core;
using JryVideo.Core.Douban;

namespace JryVideo.Add.SelectSeries
{
    public sealed class SelectSeriesViewModel : JasilyViewModel
    {
        private string _filterText;
        private SeriesViewModel _selected;

        public SelectSeriesViewModel()
        {
            this.SeriesList = new ObservableCollection<SeriesViewModel>();
            this.SeriesView = new ListCollectionView(this.SeriesList)
            {
                Filter = new Predicate<object>(this.OnFilter)
            };
        }

        private bool OnFilter(object obj)
        {
            if (String.IsNullOrWhiteSpace(this.FilterText)) return true;

            var vm = obj as SeriesViewModel;

            if (vm == null) return true;

            var keyword = this.FilterText.Trim().ToLower();

            return vm.Source.Names.Any(z => z.ToLower().Contains(keyword));
        }

        public string FilterText
        {
            get { return this._filterText; }
            set
            {
                if (this.SetPropertyRef(ref this._filterText, value))
                    this.LazyFilter();
            }
        }

        private async void LazyFilter()
        {
            var text = this.FilterText;

            await Task.Delay(400);

            if (text == this.FilterText)
            {
                this.SeriesView.Refresh();
            }
        }

        public ObservableCollection<SeriesViewModel> SeriesList { get; private set; }

        public ListCollectionView SeriesView { get; private set; }

        public SeriesViewModel Selected
        {
            get { return this._selected; }
            set { this.SetPropertyRef(ref this._selected, value); }
        }

        public async Task LoadAsync()
        {
            var seriesManager = JryVideoCore.Current.CurrentDataCenter.SeriesManager;

            this.SeriesList.AddRange(
                await Task.Run(async () =>
                    (await seriesManager.LoadAsync())
                    .Select(z => new SeriesViewModel(z))));
        }

        public DoubanMovie DoubanMovie { get; private set; }

        public async Task LoadDoubanAsync(string doubanId)
        {
            if (String.IsNullOrWhiteSpace(doubanId)) return;

            var info = await DoubanHelper.TryGetMovieInfoAsync(doubanId);

            if (info != null)
            {
                this.DoubanMovie = info;
            }
        }
    }
}