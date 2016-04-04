using Jasily.Diagnostics;
using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Selectors.SeriesSelector
{
    public sealed class SeriesSelectorViewModel : BaseSelectorViewModel<SeriesViewModel, JrySeries>
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
    }
}