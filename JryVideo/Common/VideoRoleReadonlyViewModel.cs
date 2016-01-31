using JryVideo.Model;
using JryVideo.Viewer.ArtistViewer;
using System.Windows;

namespace JryVideo.Common
{
    public class VideoRoleReadonlyViewModel : HasCoverViewModel<JryVideoRole>
    {
        public VideoRoleReadonlyViewModel(JryVideoRole source, string collectionId)
            : base(source)
        {
            this.CollectionId = collectionId;
            this.NameViewModel = new NameableViewModel<JryVideoRole>(source);
        }

        public NameableViewModel<JryVideoRole> NameViewModel { get; }

        public virtual string CollectionId { get; }

        public void ShowActor(Window window)
        {
            if (this.Source.ArtistId == null) return;
            var w = new ArtistViewerWindow() { Owner = window };
            w.ViewModel.LoadAsync(this.Source.ArtistId);
            w.ShowDialog();
        }
    }
}