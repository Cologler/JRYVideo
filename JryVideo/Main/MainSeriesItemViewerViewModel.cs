using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Model;
using JryVideo.Viewer.SeriesItemViewer;

namespace JryVideo.Main
{
    public class MainSeriesItemViewerViewModel : SeriesItemViewerViewModel
    {
        private string searchText;
        private string filterText;
        private bool hasLast;
        private bool hasNext;
        private bool isOnlyTracking;
        private SearchResult searchResultView;

        public MainSeriesItemViewerViewModel()
        {
            this.isOnlyTracking = true;
            this.PageSize = 50;
        }

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
            set { this.SetPropertyRef(ref this.isOnlyTracking, value); }
        }

        private async Task<IEnumerable<VideoInfoViewModel>> GetSourceAsync()
        {
            this.HasLast = this.HasNext = false;


            var dataCenter = JryVideoCore.Current.CurrentDataCenter;
            var search = this.searchResultView;
            if (search != null && search.Equals(dataCenter, this.IsOnlyTracking, this.SearchText, this.PageIndex, this.PageSize))
            {
                return null;
            }

            search = this.IsOnlyTracking
                ? await SearchResult.OnlyTrackingAsync(dataCenter)
                : await SearchResult.SearchAsync(dataCenter, this.SearchText, this.PageIndex, this.PageSize);

            this.searchResultView = search;

            this.HasLast = search.HasLast;
            this.HasNext = search.HasNext;

            // ReSharper disable once PossibleNullReferenceException
            this.VideosView.View.GroupDescriptions.Clear();
            if (search.IsOnlyTracking)
            {
                this.VideosView.View.CustomSort = new VideoInfoViewModel.DayOfWeekComparer();//new DayOfWeekComparer();
                this.VideosView.View.GroupDescriptions.Add(new PropertyGroupDescription("DayOfWeek"));
            }
            else
            {
                this.VideosView.View.CustomSort = Comparer<VideoInfoViewModel>.Create(this.CompareVideoInfoViewModel);
            }

            return search.Items.SelectMany(VideoInfoViewModel.Create).ToArray();
        }

        private int CompareVideoInfoViewModel(VideoInfoViewModel x, VideoInfoViewModel y)
        {
            Debug.Assert(x != null, "x != null");
            Debug.Assert(y != null, "y != null");

            return x.Source.IsTracking != y.Source.IsTracking
                ? (x.Source.IsTracking ? -1 : 1)
                : y.Source.Created.CompareTo(x.Source.Created);
        }

        private class DayOfWeekComparer : Comparer<VideoInfoViewModel>
        {
            private readonly DayOfWeek DayOfWeek = DateTime.Now.DayOfWeek;

            /// <summary>
            /// 在派生类中重写时，对同一类型的两个对象执行比较并返回一个值，指示一个对象是小于、等于还是大于另一个对象。
            /// </summary>
            /// <returns>
            /// 一个有符号整数，指示 <paramref name="x"/> 与 <paramref name="y"/> 的相对值，如下表所示。 值 含义 小于零 <paramref name="x"/> 小于 <paramref name="y"/>。 零 <paramref name="x"/> 等于 <paramref name="y"/>。 大于零 <paramref name="x"/> 大于 <paramref name="y"/>。
            /// </returns>
            /// <param name="x">要比较的第一个对象。</param><param name="y">要比较的第二个对象。</param><exception cref="T:System.ArgumentException">类型 <paramref name="T"/> 没有实现 <see cref="T:System.IComparable`1"/> 泛型接口或 <see cref="T:System.IComparable"/> 接口。</exception>
            public override int Compare(VideoInfoViewModel x, VideoInfoViewModel y)
            {
                Debug.Assert(x != null, "x != null");
                Debug.Assert(y != null, "y != null");

                if (x.Source.StartDate.HasValue && y.Source.StartDate.HasValue &&
                    (x.Source.StartDate.Value > DateTime.Now || y.Source.StartDate.Value > DateTime.Now))
                {
                    return Compare(x.Source.StartDate.Value, y.Source.StartDate.Value);
                }
                else if (x.Source.DayOfWeek != y.Source.DayOfWeek)
                {
                    return this.Compare(x.Source.DayOfWeek, y.Source.DayOfWeek);
                }
                else
                {
                    return y.Source.Created.CompareTo(x.Source.Created);
                }
            }

            private static int Compare(DateTime dt1, DateTime dt2)
            {
                if (dt1 > DateTime.Now)
                {
                    if (dt2 > DateTime.Now)
                    {
                        return DateTime.Compare(dt1, dt2);
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (dt2 > DateTime.Now)
                {
                    return -1;
                }
                else
                {
                    throw new Exception();
                }
            }

            private int Compare(DayOfWeek? d1, DayOfWeek? d2)
            {
                if (d1 == this.DayOfWeek) return -1;
                if (d2 == this.DayOfWeek) return 1;

                if (d1 == null) return -1;
                if (d2 == null) return 1;

                var sub1 = ((int) d1) - ((int) this.DayOfWeek);
                var sub2 = ((int) d2) - ((int) this.DayOfWeek);

                return sub1 * sub2 > 0 ? sub1 - sub2 : sub2 - sub1;
            }
        }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

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

            if (string.IsNullOrWhiteSpace(text))
                return !this.IsOnlyTracking || obj.Source.IsTracking;

            text = text.Trim();

            return (!this.IsOnlyTracking || obj.Source.IsTracking) &&
                   (obj.SeriesView.Source.Id == text ||
                    obj.Source.Id == text ||
                    obj.Source.Names.Concat(obj.SeriesView.Source.Names)
                        .Any(z => z.Contains(text)));
        }

        public async override Task RefreshAsync()
        {
            var source = await this.GetSourceAsync();

            if (source != null)
            {
                this.VideosView.Collection.Clear();
                this.VideosView.Collection.AddRange(source);
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

            public async Task LoadAsync(SeriesManager manager)
            {
                var items = await this.QuerySeriesAsync(manager);

                this.HasNext = !this.IsOnlyTracking && items.Count > this.PageSize;
                if (this.HasNext) items.RemoveAt(items.Count - 1);

                this.Items = items;
            }

            private async Task<List<JrySeries>> QuerySeriesAsync(SeriesManager manager)
            {
                if (this.IsOnlyTracking)
                    return await Task.Run(async () => (
                        await manager.ListTrackingAsync()).ToList());

                if (this.SearchText.IsNullOrWhiteSpace())
                    return await Task.Run(async () => (
                        await manager.LoadAsync(this.PageIndex * this.PageSize, this.PageSize + 1)).ToList());
                else
                    return await Task.Run(async () => (
                        await manager.QueryAsync(this.SearchText, this.PageIndex * this.PageSize, this.PageSize + 1)).ToList());
            }

            public bool Equals(DataCenter dataCenter, bool isOnlyTracking, string searchText, int pageIndex, int pageSize)
            {
                if (dataCenter != this.DataCenter) return false;

                if (isOnlyTracking) return this.IsOnlyTracking;

                if (pageIndex != this.PageIndex || pageSize != this.PageSize) return false;
                if (searchText.IsNullOrWhiteSpace()) return this.SearchText.IsNullOrWhiteSpace();
                return searchText.Trim() == this.SearchText;
            }
        }
    }
}