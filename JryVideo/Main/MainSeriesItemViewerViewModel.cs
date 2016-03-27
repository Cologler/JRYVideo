using Jasily.Diagnostics;
using JryVideo.Common;
using JryVideo.Core.Managers;
using JryVideo.Model;
using JryVideo.Selectors.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

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

            var ver = dataCenter.Journal.Version;
            search = this.IsOnlyTracking
                ? await SearchResult.OnlyTrackingAsync(dataCenter)
                : await SearchResult.SearchAsync(dataCenter, this.SearchText, this.PageIndex, this.PageSize);

            this.searchResultView = search;

            this.HasLast = search.HasLast;
            this.HasNext = search.HasNext;

            JasilyDebug.Pointer();
            var svm = search.Items.Select(z => new SeriesViewModel(z, ver)).ToArray();
            svm.ForEach(z =>
            {
                z.NameViewModel.IsBuildPinyin = true;
                z.NameViewModel.BeginRebuildPinyins();
            });
            var r = svm.SelectMany(z => z.VideoViewModels)
                .Where(z => this.searchResultView.IsMatch(z.SeriesView, z))
                .ToArray();
            if (this.IsOnlyTracking)
            {
                this.RebuildGroupFactoryAndRefreshItems(r);
            }
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
                item.NameViewModel.IsBuildPinyin = true;
                item.RefreshProperties();
            }
        }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        protected override void OnResetFilter(string filterText)
        {
            base.OnResetFilter(filterText);
            this.filter = new FilterInfo(this.IsOnlyTracking, filterText);
        }

        public async Task ReloadAsync()
        {
            var source = await this.GetSourceAsync();

            if (source != null)
            {
                this.OnResetFilter(this.FilterText);
                this.Items.View.CustomSort = null;
                this.Items.View.GroupDescriptions?.Clear();
                this.Items.Collection.Reset(source);
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
                    this.filterText = text.Trim();
                }
            }

            public bool Where(VideoInfoViewModel obj)
            {
                if (this.isOnlyTracking && !obj.Source.IsTracking) return false;
                if (this.filterText == null) return true;

                return
                    this.filterText.Equals(obj.SeriesView.Source.Id, StringComparison.OrdinalIgnoreCase) ||
                    this.filterText.Equals(obj.Source.Id, StringComparison.OrdinalIgnoreCase) ||
                    obj.Source.Names
                        .Concat(obj.SeriesView.Source.Names)
                        .Concat(obj.NameViewModel.Pinyins)
                        .Concat(obj.SeriesView.NameViewModel.Pinyins)
                        .Any(z => z.Contains(this.filterText, StringComparison.OrdinalIgnoreCase));
            }
        }

        private class SearchResult
        {
            private string searchText;
            private bool isOnlyTracking;
            private int pageIndex;
            private int pageSize;

            public bool HasLast => this.pageIndex > 0;

            public bool HasNext { get; private set; }

            public List<JrySeries> Items { get; private set; }

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

            private async Task<IEnumerable<JrySeries>> BuildQueryAsync(SeriesManager manager)
            {
                this.query = manager.GetQuery(this.searchText);
                return await this.query.StartQuery(this.pageIndex * this.pageSize, this.pageSize + 1);
            }

            public bool IsMatch(JrySeries series, JryVideoInfo video) => this.query?.IsMatch(series, video) ?? true;

            public bool IsSearchTextEquals(string searchText)
            {
                if (searchText.IsNullOrWhiteSpace()) return this.searchText.IsNullOrWhiteSpace();
                return searchText.Trim() == this.searchText;
            }
        }
    }
}