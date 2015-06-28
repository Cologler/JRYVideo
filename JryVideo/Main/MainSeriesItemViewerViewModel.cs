using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Common;
using JryVideo.Viewer.SeriesItemViewer;

namespace JryVideo.Main
{
    public class MainSeriesItemViewerViewModel : SeriesItemViewerViewModel
    {
        private string searchText;
        private string filterText;

        public string FilterText
        {
            get { return this.filterText; }
            set
            {
                if (this.SetPropertyRef(ref this.filterText, value))
                    this.BeginDelayFilter();
            }
        }

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                if (this.SetPropertyRef(ref this.searchText, value))
                    this.BeginDelaySearch();
            }
        }

        public async void BeginDelaySearch()
        {
            
        }

        public async void BeginDelayFilter()
        {
            var text = this.FilterText;
            await Task.Delay(400);
            if (text == this.filterText)
            {
                this.VideosView.View.Refresh();
            }
        }

        protected override bool ItemFilter(VideoInfoViewModel obj)
        {
            var text = this.FilterText;
            if (string.IsNullOrWhiteSpace(text)) return true;
            text = text.Trim();
            return obj.Source.Names.Concat(obj.SeriesView.Source.Names).Any(z => z.Contains(text));
        }

        public async override Task LoadAsync()
        {
            var manager = JryVideo.Core.JryVideoCore.Current.CurrentDataCenter.SeriesManager;

            var series = await manager.LoadAsync();

            this.VideosView.Collection.Clear();
            this.VideosView.Collection.AddRange(
                await Task.Run(() => series.SelectMany(VideoInfoViewModel.Create).ToArray()));
        }
    }
}