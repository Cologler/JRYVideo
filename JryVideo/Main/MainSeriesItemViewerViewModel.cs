using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Common;
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

            if (this.currentPageIndex == this.PageIndex && this.currentIsOnlyTracking == this.IsOnlyTracking)
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

            if (this.IsOnlyTracking && !this.currentIsOnlyTracking) // 从 not tracking 到 tracking
            {
                var pageIndex = this.PageIndex = 0; // 下次就重新回到第一页

                sources = await this.QuerySeriesAsync(pageIndex);
                this.HasLast = false;
                this.HasNext = false;

                this.VideosView.View.CustomSort = new Comparer();
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

                this.VideosView.View.CustomSort = null;
            }

            this.currentIsOnlyTracking = this.IsOnlyTracking;
            return sources.SelectMany(VideoInfoViewModel.Create).ToArray();
        }

        private class Comparer : Comparer<VideoInfoViewModel>
        {
            private DayOfWeek DayOfWeek = DateTime.Now.DayOfWeek;

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

                if (x.Source.Id == y.Source.Id) return 0;

                var v = this.Compare(x.Source.DayOfWeek, y.Source.DayOfWeek);

                if (v != 0) return -v;

                return -(-1);
            }

            private int Compare(DayOfWeek? d1, DayOfWeek? d2)
            {
                if (d1 == this.DayOfWeek) return -1;
                if (d2 == this.DayOfWeek) return -1;

                if (d1 == null) return -1;
                if (d2 == null) return -1;

                var sub1 = ((int) d1) - ((int) this.DayOfWeek);
                var sub2 = ((int) d2) - ((int) this.DayOfWeek);

                return sub1 * sub2 > 0 ? sub1 - sub2 : (sub1 < sub2 ? 1 : -1);
            }
        }

        private async Task<JrySeries[]> QuerySeriesAsync(int pageIndex, string searchText = null)
        {
            var manager = Core.JryVideoCore.Current.CurrentDataCenter.SeriesManager;

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
            if (this.IsOnlyTracking)
            {
                return obj.Source.IsTracking;
            }
            var text = this.FilterText;
            if (string.IsNullOrWhiteSpace(text)) return true;
            text = text.Trim();
            return obj.Source.Names.Concat(obj.SeriesView.Source.Names).Any(z => z.Contains(text));
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