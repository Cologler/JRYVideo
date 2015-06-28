using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace JryVideo.Common
{
    public sealed class VideoViewModel : JasilyViewModel<Model.JryVideo>
    {
        public JasilyCollectionView<EntityViewModel> EntityViews { get; private set; }

        public VideoViewModel(Model.JryVideo source)
            : base(source)
        {
            this.EntityViews = new JasilyCollectionView<EntityViewModel>();
            this.EntityViews.Collection.AddRange(source.Entities.Select(z => new EntityViewModel(z)));
        }
    }
}