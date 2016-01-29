using JryVideo.Common;
using JryVideo.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Viewer.SeriesItemViewer
{
    public class SingleSeriesItemViewerViewModel : SeriesItemViewerViewModel
    {
        private JrySeries source;

        public SingleSeriesItemViewerViewModel(JrySeries series)
        {
            this.source = series;
        }

        public JrySeries Source
        {
            get { return this.source; }
            set { this.SetPropertyRef(ref this.source, value); }
        }

        public override async Task RefreshAsync()
        {
            var series = this.Source;
            Debug.Assert(series != null);
            this.VideosView.Collection.AddRange(await Task.Run(() => VideoInfoViewModel.Create(series).ToArray()));
        }
    }
}