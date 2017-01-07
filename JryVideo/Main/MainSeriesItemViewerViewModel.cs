using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Jasily.Diagnostics;
using JryVideo.Common;
using JryVideo.Core.Managers;
using JryVideo.Model;
using JryVideo.Selectors.Common;

namespace JryVideo.Main
{
    public sealed class MainSeriesItemViewerViewModel : BaseSelectorViewModel<VideoInfoViewModel, JryVideoInfo>
    {
        private string searchText;
        private bool hasLast;
        private bool hasNext;
        private bool isOnlyTracking;
        private SearchResult searchResultView;
        private FilterInfo filter;
        private SeriesViewModel[] serieses;

        public MainSeriesItemViewerViewModel()
        {
            this.IsOnlyTracking = true;
            this.PageSize = 50;
        }

        protected override bool OnFilter(VideoInfoViewModel obj) => this.filter?.Where(obj) != false;

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
            if (this.Items.Collection.Remove(e)) this.Items.Collection.Add(e);
        }

        private async Task<IEnumerable<VideoInfoViewModel>> GetSourceAsync()
        {
            this.HasLast = this.HasNext = false;

            var dataCenter = this.GetManagers();
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

            JasilyDebug.Pointer();
            var svm = search.Items.Select(z => new SeriesViewModel(z)).ToArray();
            svm.ForEach(z =>
            {
                z.NameViewModel.IsBuildQueryStrings = true;
                z.NameViewModel.BeginRebuildQueryStrings();
            });
            var r = svm.SelectMany(z => z.VideoViewModels)
                .Where(z => this.searchResultView.IsMatch(z.SeriesView, z))
                .ToArray();
            if (this.IsOnlyTracking)
            {
                this.RebuildGroupFactoryAndRefreshItems(r);
            }
            this.serieses = svm;
            JasilyDebug.Pointer();

            return r;
        }

        public void RefreshAll() => this.RebuildGroupFactoryAndRefreshItems(this.Items.Collection);

        private void RebuildGroupFactoryAndRefreshItems(IEnumerable<VideoInfoViewModel> items)
        {
            Debug.Assert(items != null);
            VideoInfoViewModel.GroupFactory.RebuildCurrent();
            foreach (var item in items)
            {
                item.NeedGroup = true;
                item.NameViewModel.IsBuildQueryStrings = true;
                item.RefreshProperties();
            }
        }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        protected override bool OnResetFilter(string filterText)
        {
            var filter = this.filter;
            var next = new FilterInfo(this.IsOnlyTracking, filterText);
            if (filter == null || !filter.Equals(next))
            {
                this.filter = next;
                return true;
            }
            return false;
        }

        public async Task ReloadAsync()
        {
            var items = await this.GetSourceAsync();
            JasilyDebug.Pointer();

            if (items != null)
            {
                this.OnResetFilter(this.FilterText);
                this.Items.View.CustomSort = null;
                this.Items.View.GroupDescriptions?.Clear();
                this.Items.Collection.Clear();
                if (this.IsOnlyTracking)
                {
                    this.Items.View.CustomSort = new VideoInfoViewModel.GroupComparer();
                    this.Items.View.GroupDescriptions?.Add(
                        new PropertyGroupDescription(nameof(VideoInfoViewModel.VideoGroup) + "." + nameof(VideoInfoViewModel.Group.Title)));
                }
                else
                {
                    this.Items.View.CustomSort = new VideoInfoViewModel.DefaultComparer();
                }
                this.Items.Collection.AddRange(items);
                this.Items.View.Refresh();
            }
        }

        public void AllObsoleted() => this.serieses?.ForEach(z => z.SetObsoleted());

        private class FilterInfo : IEquatable<FilterInfo>
        {
            public bool OnlyTracking { get; }

            public string Text { get; }

            public FilterInfo(bool isOnlyTracking, string text)
            {
                this.OnlyTracking = isOnlyTracking;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    this.Text = text.Trim().ToUpper();
                }
            }

            public bool Where(VideoInfoViewModel obj)
                => (!this.OnlyTracking || obj.Source.IsTracking) &&
                (this.Text == null || obj.QueryFilter(this.Text));

            public bool Equals(FilterInfo other)
                => other != null && other.OnlyTracking == this.OnlyTracking && other.Text == this.Text;
        }

        private class SearchResult
        {
            private string searchText;
            private bool isOnlyTracking;
            private int pageIndex;
            private int pageSize;

            public bool HasLast => this.pageIndex > 0;

            public bool HasNext { get; private set; }

            public List<Series> Items { get; private set; }

            private SearchResult()
            {
            }

            public static async Task<SearchResult> OnlyTrackingAsync(DataCenter dataCenter)
            {
                var result = new SearchResult()
                {
                    isOnlyTracking = true
                };
                await result.InitializeQueryAsync(dataCenter.SeriesManager);
                return result;
            }

            public static async Task<SearchResult> SearchAsync(DataCenter dataCenter, string searchText, int pageIndex, int pageSize)
            {
                var result = new SearchResult()
                {
                    searchText = searchText?.Trim() ?? string.Empty,
                    pageIndex = pageIndex,
                    pageSize = pageSize
                };
                await result.InitializeQueryAsync(dataCenter.SeriesManager);
                return result;
            }

            private async Task InitializeQueryAsync(SeriesManager manager)
            {
                JasilyDebug.Pointer();
                var items = this.isOnlyTracking
                    ? await Task.Run(async () => (await manager.ListTrackingAsync()).ToList())
                    : await Task.Run(async () => (await this.BuildQueryAsync(manager)).ToList());
                JasilyDebug.Pointer();

                this.HasNext = !this.isOnlyTracking && items.Count > this.pageSize;
                if (this.HasNext) items.RemoveAt(items.Count - 1);

                this.Items = items;
            }

            private SeriesManager.Query query;

            private async Task<IEnumerable<Series>> BuildQueryAsync(SeriesManager manager)
            {
                this.query = manager.GetQuery(this.searchText);
                return await this.query.StartQuery(this.pageIndex * this.pageSize, this.pageSize + 1);
            }

            public bool IsMatch(Series series, JryVideoInfo video) => this.query?.IsMatch(series, video) ?? video.IsTracking;

            public bool IsSearchTextEquals(string searchText)
            {
                if (searchText.IsNullOrWhiteSpace()) return this.searchText.IsNullOrWhiteSpace();
                return searchText.Trim() == this.searchText;
            }
        }
    }
}