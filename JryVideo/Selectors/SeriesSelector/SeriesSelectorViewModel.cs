using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasily.Diagnostics;
using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.Common;

namespace JryVideo.Selectors.SeriesSelector
{
    public sealed class SeriesSelectorViewModel : BaseSelectorViewModel<SeriesViewModel, Series>
    {
        protected override bool OnFilter(SeriesViewModel obj)
        {
            if (!base.OnFilter(obj)) return false;

            if (String.IsNullOrWhiteSpace(this.FilterText)) return true;

            if (obj == null) return true;

            var keyword = this.FilterText.Trim().ToLower();

            return
                obj.Source.Names.Any(z => z.ToLower().Contains(keyword)) ||
                obj.Source.Videos.Any(z => z.DoubanId == keyword);
        }

        public async Task LoadAsync()
        {
            var seriesManager = this.GetManagers().SeriesManager;

            JasilyDebug.Pointer();
            this.Items.Collection.AddRange(
                await Task.Run(async () =>
                    (await seriesManager.LoadAsync())
                    .Select(z => new SeriesViewModel(z))));
            JasilyDebug.Pointer();
        }

        public void Remove(SeriesViewModel series)
        {
            if (series.Source.Videos.Count > 0) throw new Exception();
            this.Items.Collection.Remove(series);
            var seriesManager = this.GetManagers().SeriesManager;
            seriesManager.RemoveAsync(series.Source.Id);
        }
    }
}