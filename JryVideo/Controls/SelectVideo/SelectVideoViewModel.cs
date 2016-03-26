using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Controls.SelectVideo
{
    public sealed class SelectVideoViewModel : BaseSelectorViewModel<VideoInfoViewModel, JryVideoInfo>
    {
        private JrySeries source;

        public JrySeries Source
        {
            get { return this.source; }
            set
            {
                Debug.Assert(value != null, "value != null");
                this.SetPropertyRef(ref this.source, value);
            }
        }

        public string DefaultId { get; set; }

        public async Task RefreshAsync()
        {
            var series = this.Source;
            Debug.Assert(series != null);
            this.Items.Collection.AddRange(await Task.Run(() => VideoInfoViewModel.Create(series).ToArray()));
            if (this.DefaultId != null)
            {
                this.Items.Selected = this.Items.Collection.FirstOrDefault(z => z.Source.Id == this.DefaultId);
            }
        }
    }
}