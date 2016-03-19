using JryVideo.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JryVideo.Controls.SelectFlag
{
    public sealed class SelectFlagViewModel
    {
        public JryFlagType Type { get; }

        public ObservableCollection<string> Collection { get; } = new ObservableCollection<string>();

        public SelectFlagViewModel(JryFlagType type)
        {
            this.Type = type;
        }

        public void ReadTags(ITagable item)
        {
            if (item.Tags != null && item.Tags.Count > 0)
            {
                this.Collection.AddRange(item.Tags);
            }
        }

        public void WriteTags(ITagable item, bool nullIfEmpty)
            => item.Tags = this.Collection.Count == 0 && nullIfEmpty ? null : this.Collection.ToList();
    }
}