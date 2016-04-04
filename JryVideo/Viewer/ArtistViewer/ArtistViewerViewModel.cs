using Jasily.ComponentModel;
using JryVideo.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JryVideo.Viewer.ArtistViewer
{
    public class ArtistViewerViewModel : JasilyViewModel
    {
        private ArtistViewModel artistView;

        public ArtistViewModel ArtistView
        {
            get { return this.artistView; }
            private set { this.SetPropertyRef(ref this.artistView, value); }
        }

        public async void LoadAsync(string artistId)
        {
            this.ArtistView = null;
            this.RoleViews.Clear();

            var dataCenter = this.GetManagers();
            var artist = await dataCenter.ArtistManager.FindAsync(artistId);
            if (artist == null) return;
            this.ArtistView = new ArtistViewModel(artist);

            var roles = (await dataCenter.VideoRoleManager.QueryByActorIdAsync(artistId))
                .Select(z => new VideoRoleReadonlyViewModel(z.Item2))
                .ToArray();
            this.RoleViews.AddRange(roles);
        }

        public ObservableCollection<VideoRoleReadonlyViewModel> RoleViews { get; }
            = new ObservableCollection<VideoRoleReadonlyViewModel>();
    }
}