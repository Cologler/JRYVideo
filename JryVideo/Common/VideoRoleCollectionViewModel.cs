using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Core;
using JryVideo.Model;
using JryVideo.Viewer.VideoViewer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Common
{
    public class VideoRoleCollectionViewModel : JasilyViewModel
    {
        private readonly string videoId;

        public VideoRoleCollectionViewModel(string videoId, VideoViewerViewModel videoViewerViewModel = null)
        {
            this.videoId = videoId;
            this.VideoViewerViewModel = videoViewerViewModel;
        }

        public VideoViewerViewModel VideoViewerViewModel { get; }

        public JasilyCollectionView<VideoRoleViewModel> Roles { get; }
            = new JasilyCollectionView<VideoRoleViewModel>();

        public VideoRoleCollection RoleSource { get; private set; }

        public async void BeginLoad()
        {
            this.Roles.Collection.Clear();

            this.RoleSource = await JryVideoCore.Current.CurrentDataCenter.VideoRoleManager.FindAsync(this.videoId);
            if (this.RoleSource != null)
            {
                if (this.RoleSource.MajorRoles != null)
                {
                    this.Roles.Collection.AddRange(
                        this.RoleSource.MajorRoles.Select(z => new VideoRoleViewModel(z, this, true)));
                }

                if (this.RoleSource.MinorRoles != null)
                {
                    this.Roles.Collection.AddRange(
                        this.RoleSource.MinorRoles.Select(z => new VideoRoleViewModel(z, this, false)));
                }
            }
        }

        public async Task CommitAsync()
        {
            if (this.RoleSource == null) return;
            await JryVideoCore.Current.CurrentDataCenter.VideoRoleManager.UpdateAsync(this.RoleSource);
        }
    }
}