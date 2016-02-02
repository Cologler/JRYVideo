using JryVideo.Model;
using JryVideo.Viewer.ArtistViewer;
using System.Linq;
using System.Windows;

namespace JryVideo.Common
{
    public class VideoRoleReadonlyViewModel : HasCoverViewModel<JryVideoRole>
    {
        private string actorName;

        public VideoRoleReadonlyViewModel(JryVideoRole source, string collectionId)
            : base(source)
        {
            this.CollectionId = collectionId;
            this.NameViewModel = new NameableViewModel<JryVideoRole>(source);
        }

        public NameableViewModel<JryVideoRole> NameViewModel { get; }

        public virtual string CollectionId { get; }

        public string ActorName
        {
            get { return this.actorName; }
            private set { this.SetPropertyRef(ref this.actorName, value); }
        }

        public string RoleName
        {
            get
            {
                var name = this.Source.RoleName?.FirstOrDefault();
                return name == null ? string.Empty : $"as {name}";
            }
        }

        /// <summary>
        /// the method will call PropertyChanged for each property which has [NotifyPropertyChanged]
        /// </summary>
        public override async void RefreshProperties()
        {
            base.RefreshProperties();

            var actor = await this.GetManagers().ArtistManager.FindAsync(this.Source.ArtistId);
            this.ActorName = actor?.GetMajorName() ?? string.Empty;
        }

        public void ShowActor(Window window)
        {
            if (this.Source.ArtistId == null) return;
            var w = new ArtistViewerWindow() { Owner = window };
            w.ViewModel.LoadAsync(this.Source.ArtistId);
            w.ShowDialog();
        }
    }
}