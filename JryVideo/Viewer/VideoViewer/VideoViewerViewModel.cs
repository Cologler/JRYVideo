using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using JryVideo.Common;
using JryVideo.Core;

namespace JryVideo.Viewer.VideoViewer
{
    public sealed class VideoViewerViewModel : JasilyViewModel
    {
        private VideoViewModel video;

        public VideoViewerViewModel(VideoInfoViewModel info)
        {
            this.Info = info;
            this.EntitesView = new JasilyCollectionView<ObservableCollectionGroup<string, EntityViewModel>>();
        }

        public VideoInfoViewModel Info { get; private set; }

        public VideoViewModel Video
        {
            get { return this.video; }
            private set { this.SetPropertyRef(ref this.video, value); }
        }

        public async Task LoadAsync()
        {
            await this.ReloadVideoAsync();
        }

        public async Task ReloadVideoAsync()
        {
            var manager = JryVideoCore.Current.CurrentDataCenter.VideoManager;

            var video = await manager.FindAsync(this.Info.Source.Id);

            if (video == null)
            {
                this.Video = null;
            }
            else
            {
                this.Video = new VideoViewModel(video);

                this.EntitesView.Collection.Clear();
                this.EntitesView.Collection.AddRange(video.Entities
                    .Select(z => new EntityViewModel(z))
                    .GroupBy(v => v.Source.Resolution ?? "unknown")
                    .Select(g =>
                    {
                        var l = g.ToList();
                        l.Sort(new EntityViewModelComparer());
                        return new ObservableCollectionGroup<string, EntityViewModel>(g.Key, l);
                    }));
            }
        }

        public JasilyCollectionView<ObservableCollectionGroup<string, EntityViewModel>> EntitesView { get; private set; }

        private class EntityViewModelComparer : Comparer<EntityViewModel>
        {
            /// <summary>
            /// 在派生类中重写时，对同一类型的两个对象执行比较并返回一个值，指示一个对象是小于、等于还是大于另一个对象。
            /// </summary>
            /// <returns>
            /// 一个有符号整数，指示 <paramref name="x"/> 与 <paramref name="y"/> 的相对值，如下表所示。 值 含义 小于零 <paramref name="x"/> 小于 <paramref name="y"/>。 零 <paramref name="x"/> 等于 <paramref name="y"/>。 大于零 <paramref name="x"/> 大于 <paramref name="y"/>。
            /// </returns>
            /// <param name="x">要比较的第一个对象。</param><param name="y">要比较的第二个对象。</param><exception cref="T:System.ArgumentException">类型 <paramref name="T"/> 没有实现 <see cref="T:System.IComparable`1"/> 泛型接口或 <see cref="T:System.IComparable"/> 接口。</exception>
            public override int Compare(EntityViewModel x, EntityViewModel y)
            {
                Debug.Assert(x != null, "x != null");
                Debug.Assert(y != null, "y != null");

                if (x.Source.Id == y.Source.Id) return 0;
                
                if (x.Source.Fansubs.Count > 0 && y.Source.Fansubs.Count > 0)
                    return Comparer<string>.Default.Compare(x.Source.Fansubs[0], y.Source.Fansubs[0]);

                if (x.Source.Extension != y.Source.Extension)
                    return Comparer<string>.Default.Compare(x.Source.Extension, y.Source.Extension);

                return -1;
            }
        }
    }
}