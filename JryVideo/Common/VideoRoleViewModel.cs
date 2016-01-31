using JryVideo.Core;
using JryVideo.Model;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Common
{
    public class VideoRoleViewModel : HasCoverViewModel<JryVideoRole>
    {
        private readonly VideoRoleCollectionViewModel parent;
        private readonly IImdbItem imdbItem;

        public VideoRoleViewModel(JryVideoRole source, VideoRoleCollectionViewModel parent, IImdbItem imdbItem, bool isMajor)
            : base(source)
        {
            this.parent = parent;
            this.imdbItem = imdbItem;
            this.IsMajor = isMajor;
            this.NameViewModel = new NameableViewModel<JryVideoRole>(source);
        }

        public NameableViewModel<JryVideoRole> NameViewModel { get; }

        public bool IsMajor { get; }

        public string GroupTitle
        {
            get
            {
                var type = this.imdbItem is JrySeries ? "series" : "video";
                var level = this.IsMajor ? "major" : "minor";
                return $"{type} {level} actor";
            }
        }

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
            var imdb = this.imdbItem.GetValidImdb();
            if (client == null || imdb == null) return false;
            var series = (await client.GetSeriesByImdbIdAsync(imdb)).FirstOrDefault();
            if (series == null) return false;
            var actors = (await series.GetActorsAsync(client)).Where(
                z => z.Role != null && this.Source.RoleName != null
                    ? this.Source.RoleName.Contains(z.Role.Trim())
                    : this.Source.ActorName == z.Name).ToArray();
            if (actors.Length != 1) return false;
            if (!actors[0].HasBanner) return false;
            var url = actors[0].BuildUrl(client);

            var cover = JryCover.CreateRole(this.parent.VideoViewerViewModel.InfoView.Source, url, this.Source);
            var guid = await JryVideoCore.Current.CurrentDataCenter.CoverManager.DownloadCoverAsync(cover);
            if (guid != null)
            {
                this.Source.CoverId = guid;
                await this.parent.CommitAsync(this.imdbItem.Id);
                return true;
            }
            return false;
        }
    }
}