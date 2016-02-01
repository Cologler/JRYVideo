using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Viewer.SeriesItemViewer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Controls.SelectVideo
{
    public class SelectVideoViewModel : SeriesItemViewerViewModel
    {
        private JrySeries source;

        public JrySeries Source
        {
            get { return this.source; }
            set
            {
                Debug.Assert(value != null);
                this.SetPropertyRef(ref this.source, value);
            }
        }

        public JryVideoInfo Without { get; set; }

        public string DefaultId { get; set; }

        protected override bool ItemFilter(VideoInfoViewModel obj)
        {
            return base.ItemFilter(obj) && this.Without?.Id != obj.Source.Id;
        }

        public override async Task RefreshAsync()
        {
            var series = this.Source;
            Debug.Assert(series != null);
            this.VideosView.Collection.AddRange(await Task.Run(() => VideoInfoViewModel.Create(series).ToArray()));
            if (this.DefaultId != null)
            {
                this.VideosView.Selected = this.VideosView.Collection.FirstOrDefault(z => z.Source.Id == this.DefaultId);
            }
        }
    }
}