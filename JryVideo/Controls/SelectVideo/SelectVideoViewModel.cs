using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JryVideo.Controls.SelectVideo
{
    public sealed class SelectVideoViewModel : BaseSelectorViewModel<VideoInfoViewModel, JryVideoInfo>
    {
        public SeriesViewModel Series { get; private set; }

        public void SetSeries(SeriesViewModel series, string defaultId = null)
        {
            Debug.Assert(series != null);
            this.Series = series;

            this.Items.Collection.Reset(series.VideoViewModels);
            if (defaultId != null)
            {
                this.Items.Selected = this.Items.Collection.FirstOrDefault(z => z.Source.Id == defaultId);
            }
        }
    }
}