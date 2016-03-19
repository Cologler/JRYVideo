using System.Collections.ObjectModel;
using JryVideo.Model;

namespace JryVideo.Controls.SelectFlag
{
    public sealed class SelectFlagViewModel
    {
        public JryFlagType Type { get; }

        public ObservableCollection<string> Collection { get; }

        public SelectFlagViewModel(JryFlagType type, ObservableCollection<string> collection)
        {
            this.Type = type;
            this.Collection = collection;
        }
    }
}