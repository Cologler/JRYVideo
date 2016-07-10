using System.Linq;
using System.Windows;
using Jasily.ComponentModel;
using JryVideo.Model;
using JryVideo.Viewer.ArtistViewer;

namespace JryVideo.Common
{
    public class VideoRoleReadonlyViewModel : JasilyViewModel<VideoRole>
    {
        private string actorName;

        public VideoRoleReadonlyViewModel(VideoRole source)
            : base(source)
        {
            this.NameViewModel = new NameableViewModel<VideoRole>(source);
            this.CoverViewModel = new CoverViewModel(this.Source);
        }

        public NameableViewModel<VideoRole> NameViewModel { get; }

        public CoverViewModel CoverViewModel { get; }

        [NotifyPropertyChanged]
        public string ActorName
        {
            get { return this.actorName; }
            private set { this.SetPropertyRef(ref this.actorName, value); }
        }

        [NotifyPropertyChanged]
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

            var actor = await this.GetManagers().ArtistManager.FindAsync(this.Source.ActorId);
            this.ActorName = actor?.GetMajorName() ?? string.Empty;
        }

        public void ShowActor(Window window)
        {
            if (this.Source.ActorId == null) return;
            var w = new ArtistViewerWindow() { Owner = window };
            w.TitleTextBlock.Text = "actor";
            w.ViewModel.LoadAsync(this.Source.ActorId);
            w.ShowDialog();
        }
    }
}