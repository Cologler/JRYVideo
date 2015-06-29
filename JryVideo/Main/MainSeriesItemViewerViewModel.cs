using System.Collections.Generic;
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
        private bool isSearching;

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
            set { this.SetPropertyRef(ref this.searchText, value); }
        }

        public async Task SearchAsync()
        {
            var source = await this.GetSourceAsync();

            if (source != null)
            {
                this.VideosView.Collection.Clear();
                this.VideosView.Collection.AddRange(source);
            }
        }

        private async Task<IEnumerable<VideoInfoViewModel>> GetSourceAsync()
        {
            var manager = JryVideo.Core.JryVideoCore.Current.CurrentDataCenter.SeriesManager;
            var text = this.SearchText;

            if (string.IsNullOrWhiteSpace(text))
            {
                if (this.isSearching)
                {
                    return await Task.Run(async () => (await manager.LoadAsync()).SelectMany(VideoInfoViewModel.Create).ToArray());
                }

                return null;
            }
            else
            {
                return await Task.Run(async () => (await manager.QueryAsync(text)).SelectMany(VideoInfoViewModel.Create).ToArray());
            }
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
            this.isSearching = true;
            await this.SearchAsync();
        }
    }
}