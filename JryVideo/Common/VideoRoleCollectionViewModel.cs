using Jasily.ComponentModel;
using Jasily.Windows.Data;
using JryVideo.Core;
using JryVideo.Model;
using JryVideo.Viewer.VideoViewer;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace JryVideo.Common
{
    public class VideoRoleCollectionViewModel : JasilyViewModel
    {
        private readonly JrySeries series;
        private readonly JryVideoInfo video;

        public VideoRoleCollectionViewModel(JrySeries series, JryVideoInfo video)
        {
            this.series = series;
            this.video = video;

            this.Roles.View.GroupDescriptions?.Add(new PropertyGroupDescription(nameof(VideoRoleViewModel.GroupTitle)));
        }

        public VideoViewerViewModel VideoViewerViewModel { get; set; }

        public JasilyCollectionView<VideoRoleViewModel> Roles { get; }
            = new JasilyCollectionView<VideoRoleViewModel>();

        public List<VideoRoleCollection> VideoRoleCollectionSources { get; }
            = new List<VideoRoleCollection>();

        public async void BeginLoad()
        {
            this.Roles.Collection.Clear();
            this.VideoRoleCollectionSources.Clear();

            var major = new List<VideoRoleViewModel>();
            var minor = new List<VideoRoleViewModel>();
            foreach (var imdbItem in new IImdbItem[] { this.series, this.video })
            {
                var col = await JryVideoCore.Current.CurrentDataCenter.VideoRoleManager.FindAsync(imdbItem.Id);
                if (col != null)
                {
                    this.VideoRoleCollectionSources.Add(col);
                    if (col.MajorRoles != null)
                    {
                        major.AddRange(col.MajorRoles.Select(z => new VideoRoleViewModel(z, this, imdbItem, true)));
                    }
                    if (col.MinorRoles != null)
                    {
                        minor.AddRange(col.MinorRoles.Select(z => new VideoRoleViewModel(z, this, imdbItem, false)));
                    }
                }
            }
            
            this.Roles.Collection.AddRange(major);
            this.Roles.Collection.AddRange(minor);
        }

        public async Task CommitAsync(string id)
        {
            var col = this.VideoRoleCollectionSources.FirstOrDefault(z => z.Id == id);
            if (col != null)
            {
                await JryVideoCore.Current.CurrentDataCenter.VideoRoleManager.UpdateAsync(col);
            }
        }
    }
}