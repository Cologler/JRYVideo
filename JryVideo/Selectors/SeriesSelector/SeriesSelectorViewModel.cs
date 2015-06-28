using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Core;
using JryVideo.Selectors.Common;

namespace JryVideo.Selectors.SeriesSelector
{
    public sealed class SeriesSelectorViewModel : BaseSelectorViewModel<SeriesViewModel>
    {
        protected override bool OnFilter(SeriesViewModel obj)
        {
            if (String.IsNullOrWhiteSpace(this.FilterText)) return true;

            if (obj == null) return true;

            var keyword = this.FilterText.Trim().ToLower();

            return obj.Source.Names.Any(z => z.ToLower().Contains(keyword));
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