using JryVideo.Core;
using JryVideo.Model;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Common
{
    public class VideoRoleViewModel : HasCoverViewModel<JryVideoRole>
    {
        private readonly VideoRoleCollectionViewModel parent;

        public VideoRoleViewModel(JryVideoRole source, VideoRoleCollectionViewModel parent, bool isMajor)
            : base(source)
        {
            this.parent = parent;
            this.IsMajor = isMajor;
            this.NameViewModel = new NameableViewModel<JryVideoRole>(source);
        }

        public NameableViewModel<JryVideoRole> NameViewModel { get; }

        public bool IsMajor { get; }

        public string GroupTitle => this.IsMajor ? "major" : "minor";

        /// <summary>
        /// the method will call PropertyChanged for each property which has [NotifyPropertyChanged]
        /// </summary>
        public override void RefreshProperties()
        {
            base.RefreshProperties();
            this.NameViewModel.RefreshProperties();
        }

        protected override async Task<bool> TryAutoAddCoverAsync()
        {
            if (this.parent.VideoViewerViewModel == null) return false;
            var client = JryVideoCore.Current.TheTVDBClient;
            if (client == null) return false;
            var imdb = this.parent.VideoViewerViewModel.InfoView.Source.ImdbId;
            var series = (await client.GetSeriesByImdbIdAsync(imdb)).FirstOrDefault();
            if (series == null) return false;
            var actors = (await series.GetActorsAsync(client)).Where(
                z => z.Role != null && this.Source.RoleName != null
                    ? this.Source.RoleName.Contains(z.Role.Trim())
                    : this.Source.ActorName == z.Name).ToArray();
            if (actors.Length != 1) return false;
            if (!actors[0].HasBanner) return false;
            var url = actors[0].BuildUrl(client);

            var cover = new JryCover();
            cover.CoverSourceType = JryCoverSourceType.Imdb;
            cover.CoverType = JryCoverType.Role;
            cover.Uri = url;
            cover.RoleId = this.parent.VideoViewerViewModel.InfoView.Source.Id + "_" + this.Source.Id; // 分配给自己（而不是 series imdb Id）

            var guid = await JryVideoCore.Current.CurrentDataCenter.CoverManager.DownloadCoverAsync(cover);
            if (guid != null)
            {
                this.Source.CoverId = guid;
                await this.parent.CommitAsync();
                return true;
            }
            return false;
        }
    }
}