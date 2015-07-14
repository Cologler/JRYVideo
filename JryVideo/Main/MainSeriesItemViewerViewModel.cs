using System;
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
        private string currentSearchText;
        private int currentPageIndex;
        private string searchText;
        private string filterText;
        private bool hasLast;
        private bool hasNext;
        private bool currentIsOnlyTracking;
        private bool isOnlyTracking;
        private string filterId;
        private DataCenter lastDataCenter;

        public MainSeriesItemViewerViewModel()
        {
            this.isOnlyTracking = true;
            this.PageSize = 50;
            this.currentPageIndex = -1; // then init load.
            this.VideosView.View.GroupDescriptions.Add(new PropertyGroupDescription("DayOfWeek"));
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

            var text = (this.SearchText ?? "").Trim();

            if (this.currentPageIndex == this.PageIndex &&
                this.currentIsOnlyTracking == this.IsOnlyTracking &&
                JryVideoCore.Current.CurrentDataCenter == this.lastDataCenter)
            {
                if (text.IsNullOrWhiteSpace() && this.currentSearchText.IsNullOrWhiteSpace())
                {
                    return null;
                }

                if (!text.IsNullOrWhiteSpace() && text == this.currentSearchText)
                {
                    return null;
                }
            }

            JrySeries[] sources;
            this.lastDataCenter = JryVideoCore.Current.CurrentDataCenter;

            if (this.IsOnlyTracking && !this.currentIsOnlyTracking) // 从 not tracking 到 tracking
            {
                var pageIndex = this.PageIndex = 0; // 下次就重新回到第一页

                sources = await this.QuerySeriesAsync(pageIndex);
                this.HasLast = false;
                this.HasNext = false;

                this.VideosView.View.CustomSort = new DayOfWeekComparer();
            }
            else
            {

                if (text != this.currentSearchText)
                    this.PageIndex = 0;
                var pageIndex = this.PageIndex;

                sources = await this.QuerySeriesAsync(pageIndex, text);
                this.currentSearchText = text;
                this.currentPageIndex = pageIndex;
                this.HasLast = this.PageIndex > 0;
                this.HasNext = sources.Length == this.PageSize;

                this.VideosView.View.CustomSort = new DefaultComparer();
            }

            this.currentIsOnlyTracking = this.IsOnlyTracking;
            return sources.SelectMany(VideoInfoViewModel.Create).ToArray();
        }

        private class DefaultComparer : Comparer<VideoInfoViewModel>
        {
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

                if (x.Source.IsTracking != y.Source.IsTracking)
                {
                    return x.Source.IsTracking ? -1 : 1;
                }
                else
                {
                    return y.Source.Created.CompareTo(x.Source.Created);
                }
            }
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

        private async Task<JrySeries[]> QuerySeriesAsync(int pageIndex, string searchText = null)
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.SeriesManager;

            if (this.IsOnlyTracking)
                return await Task.Run(async () => (await manager.ListTrackingAsync()).ToArray());

            if (searchText.IsNullOrWhiteSpace())
                return await Task.Run(async () => (await manager.LoadAsync(pageIndex * this.PageSize, this.PageSize)).ToArray());
            else
                return await Task.Run(async () => (await manager.QueryAsync(searchText, pageIndex * this.PageSize, this.PageSize)).ToArray());
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
    }
}