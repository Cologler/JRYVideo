using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Selectors.Common;

namespace JryVideo.Controls.SelectVideo
{
    public sealed class SelectVideoViewModel : BaseSelectorViewModel<VideoInfoViewModel, JryVideoInfo>
    {
        private bool isInitialized;
        private string defaultVideoId;
        private SeriesViewModel series;

        public SelectVideoViewModel()
        {
            var grouping = PropertySelector<VideoInfoViewModel>.Start(z => z.Source.GroupIndex).ToString();
            Debug.Assert(grouping == "Source.GroupIndex");
            this.Items.View.GroupDescriptions?.Add(new PropertyGroupDescription(grouping));

            this.Items.View.CustomSort = Comparer<VideoInfoViewModel>.Create((x, y) =>
                x.Source.GroupIndex.CompareTo(y.Source.GroupIndex));
        }

        public SeriesViewModel Series
        {
            get { return this.series; }
            set
            {
                Debug.Assert(this.series == null);
                if (this.isInitialized) throw new InvalidOperationException();
                this.series = value;
            }
        }

        public string DefaultVideoId
        {
            get { return this.defaultVideoId; }
            set
            {
                if (this.isInitialized) throw new InvalidOperationException();
                this.defaultVideoId = value;
            }
        }

        public void Initialize()
        {
            this.isInitialized = true;
            if (this.Series == null) throw new InvalidOperationException();

            var groups = this.Series.VideoViewModels.Select(z => z.Source)
                .GroupBy(z => z.GroupIndex)
                .OrderBy(z => z.Key)
                .ToArray();
            var groupsViewModels = groups.Select(z => new Group(z.Key) { Count = z.Count() }).ToArray();
            this.TargetGroups.AddRange(groupsViewModels);
            this.TargetGroups.Add(new Group(this.TargetGroups.Count));

            this.Items.Collection.Reset(series.VideoViewModels);
            if (this.DefaultVideoId != null)
            {
                this.Items.Selected = this.Items.Collection.FirstOrDefault(z => z.Source.Id == this.DefaultVideoId);
            }
        }

        public bool CanMoveTo(VideoInfoViewModel video, Group group)
        {
            Debug.Assert(this.TargetGroups.Count > video.Source.GroupIndex);
            var sourceGroup = this.TargetGroups[video.Source.GroupIndex];
            Debug.Assert(sourceGroup.Index == video.Source.GroupIndex);
            if (group.Index == video.Source.GroupIndex) return false;
            return sourceGroup.Count > 1 || group.Index < video.Source.GroupIndex;
        }

        public async Task MoveToAsync(VideoInfoViewModel video, Group group)
        {
            Debug.Assert(this.CanMoveTo(video, group));
            var sourceGroup = this.TargetGroups[video.Source.GroupIndex];
            Debug.Assert(sourceGroup.Index == video.Source.GroupIndex);
            sourceGroup.Count--;
            group.Count++;
            video.Source.GroupIndex = group.Index;
            await this.Series.SaveAsync();
            if (group.Count == 1) // must be last.
            {
                Debug.Assert(group.Index == this.TargetGroups.Count - 1);
                this.TargetGroups.Add(new Group(this.TargetGroups.Count));
            }
            Debug.Assert(this.TargetGroups.Count >= 2);
            while (0 ==
                this.TargetGroups[this.TargetGroups.Count - 1].Count +
                this.TargetGroups[this.TargetGroups.Count - 2].Count)
            {
                this.TargetGroups.RemoveAt(this.TargetGroups.Count - 1);
            }
            this.Items.View.Refresh();
        }

        public ObservableCollection<Group> TargetGroups { get; } = new ObservableCollection<Group>();

        public class Group : JasilyViewModel
        {
            private int count;

            public int Index { get; }

            public Group(int index)
            {
                this.Index = index;
            }

            public int Count
            {
                get
                {
                    Debug.Assert(this.count >= 0);
                    return this.count;
                }
                set
                {
                    if (this.count.Equals(value)) return;
                    this.count = value;
                    this.NotifyPropertyChanged(nameof(this.DisplayMoveHeader));
                }
            }

            public string DisplayMoveHeader => this.Count > 0 ? $"move to group {this.Index}" : $"move to group {this.Index} (+)";
        }
    }
}