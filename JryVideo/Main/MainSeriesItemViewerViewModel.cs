using Jasily.Diagnostics;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Model;
using JryVideo.Viewer.SeriesItemViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JryVideo.Main
{
    public class MainSeriesItemViewerViewModel : SeriesItemViewerViewModel
    {
        private string searchText;
        private bool hasLast;
        private bool hasNext;
        private bool isOnlyTracking;
        private SearchResult searchResultView;
        private FilterInfo filter;

        public MainSeriesItemViewerViewModel()
        {
            this.IsOnlyTracking = true;
            this.PageSize = 50;
        }

        protected override bool OnFilter(VideoInfoViewModel obj)
            => base.OnFilter(obj) && this.filter?.Where(obj) != false;

        public string SearchText
        {
            get { return this.searchText; }
            set { this.SetPropertyRef(ref this.searchText, value); }
        }

        public bool HasLast
        {
            get { return this.hasLast; }
            set { this.SetPropertyRef(ref this.hasLast, value); }
        }

        public bool HasNext
        {
            get { return this.hasNext; }
            set { this.SetPropertyRef(ref this.hasNext, value); }
        }

        public bool IsOnlyTracking
        {
            get { return this.isOnlyTracking; }
            set
            {
                if (this.SetPropertyRef(ref this.isOnlyTracking, value))
                {
                    if (value)
                    {
                        VideoInfoViewModel.IsWatchedUpdated += this.VideoInfoViewModel_IsWatchedUpdated;
                    }
                    else
                    {
                        VideoInfoViewModel.IsWatchedUpdated -= this.VideoInfoViewModel_IsWatchedUpdated;
                    }
                }
            }
        }

        private void VideoInfoViewModel_IsWatchedUpdated(object sender, VideoInfoViewModel e)
        {
            this.VideosView.Collection.Remove(e);
            this.VideosView.Collection.Add(e);
        }

        private async Task<IEnumerable<VideoInfoViewModel>> GetSourceAsync()
        {
            this.HasLast = this.HasNext = false;

            var dataCenter = JryVideoCore.Current.CurrentDataCenter;
            var search = this.searchResultView;

            if (search == null || !search.IsSearchTextEquals(this.SearchText))
            {
                this.PageIndex = 0;
            }

            search = this.IsOnlyTracking
                ? await SearchResult.OnlyTrackingAsync(dataCenter)
                : await SearchResult.SearchAsync(dataCenter, this.SearchText, this.PageIndex, this.PageSize);

            this.searchResultView = search;

            this.HasLast = search.HasLast;
            this.HasNext = search.HasNext;

            return search.Items.SelectMany(VideoInfoViewModel.Create).ToArray();
        }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        protected override void OnResetFilter(string filterText)
        {
            base.OnResetFilter(filterText);
            this.filter = new FilterInfo(this.IsOnlyTracking, filterText);
        }

        public virtual async Task RefreshAsync()
        {
            var source = await this.GetSourceAsync();

            if (source != null)
            {
                this.filter = new FilterInfo(this.IsOnlyTracking, this.FilterText);
                this.VideosView.View.CustomSort = null;
                this.VideosView.View.GroupDescriptions?.Clear();
                this.VideosView.Collection.Reset(source);
                if (this.IsOnlyTracking)
                {
                    this.VideosView.View.CustomSort = new VideoInfoViewModel.GroupComparer();
                    this.VideosView.View.GroupDescriptions?.Add(
                        new PropertyGroupDescription(nameof(VideoInfoViewModel.VideoGroup) + "." + nameof(VideoInfoViewModel.Group.Title)));
                }
                else
                {
                    this.VideosView.View.CustomSort = new VideoInfoViewModel.DefaultComparer();
                }
            }
        }

        private class FilterInfo
        {
            private readonly bool isOnlyTracking;
            private readonly string filterText;

            public FilterInfo(bool isOnlyTracking, string text)
            {
                this.isOnlyTracking = isOnlyTracking;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    this.filterText = text.Trim().ToLower();
                }
            }

            public bool Where(VideoInfoViewModel obj)
            {
                if (this.isOnlyTracking && !obj.Source.IsTracking) return false;
                if (this.filterText == null) return true;

                return obj.SeriesView.Source.Id.ToLower() == this.filterText ||
                       obj.Source.Id.ToLower() == this.filterText ||
                       obj.Source.Names.Concat(obj.SeriesView.Source.Names)
                       .Any(z => z.ToLower().Contains(this.filterText));
            }
        }

        private class SearchResult
        {
            public string SearchText { get; private set; }

            public bool IsOnlyTracking { get; private set; }

            public int PageIndex { get; private set; }

            public int PageSize { get; private set; }

            public bool HasLast => this.PageIndex > 0;

            public bool HasNext { get; private set; }

            public DataCenter DataCenter { get; private set; }

            public List<JrySeries> Items { get; private set; }

            private SearchResult(DataCenter dataCenter)
            {
                this.DataCenter = dataCenter;
            }

            public static async Task<SearchResult> OnlyTrackingAsync(DataCenter dataCenter)
            {
                var result = new SearchResult(dataCenter)
                {
                    IsOnlyTracking = true
                };
                await result.LoadAsync(dataCenter.SeriesManager);
                return result;
            }

            public static async Task<SearchResult> SearchAsync(DataCenter dataCenter, string searchText, int pageIndex, int pageSize)
            {
                var result = new SearchResult(dataCenter)
                {
                    SearchText = searchText?.Trim() ?? string.Empty,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
                await result.LoadAsync(dataCenter.SeriesManager);
                return result;
            }

            private async Task LoadAsync(SeriesManager manager)
            {
                JasilyDebug.Pointer();
                var items = this.IsOnlyTracking
                    ? await Task.Run(async () => (await manager.ListTrackingAsync()).ToList())
                    : await Task.Run(async () => (await manager.QueryAsync(this.SearchText, this.PageIndex * this.PageSize, this.PageSize + 1)).ToList());
                JasilyDebug.Pointer();

                this.HasNext = !this.IsOnlyTracking && items.Count > this.PageSize;
                if (this.HasNext) items.RemoveAt(items.Count - 1);

                this.Items = items;
            }

            public bool IsSearchTextEquals(string searchText)
            {
                if (searchText.IsNullOrWhiteSpace()) return this.SearchText.IsNullOrWhiteSpace();
                return searchText.Trim() == this.SearchText;
            }
        }
    }
}