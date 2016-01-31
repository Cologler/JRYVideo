using JryVideo.Core;
using JryVideo.Model;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JryVideo.Editors.RoleEditor;

namespace JryVideo.Common
{
    public class VideoRoleViewModel : HasCoverViewModel<JryVideoRole>
    {
        private readonly VideoRoleCollectionViewModel parent;

        public VideoRoleViewModel(JryVideoRole source, VideoRoleCollectionViewModel parent, IImdbItem imdbItem, bool isMajor)
            : base(source)
        {
            this.parent = parent;
            this.ImdbItem = imdbItem;
            this.IsMajor = isMajor;
            this.NameViewModel = new NameableViewModel<JryVideoRole>(source);
        }

        public NameableViewModel<JryVideoRole> NameViewModel { get; }

        public IImdbItem ImdbItem { get; private set; }

        public bool IsSeriesRole => this.ImdbItem is JrySeries;

        public bool IsMajor { get; }

        public int OrderIndex => (this.IsSeriesRole ? 0 : 10) + (this.IsMajor ? 0 : 1);

        public string GroupTitle
        {
            get
            {
                var type = this.IsSeriesRole ? "series" : "video";
                var level = this.IsMajor ? "major" : "minor";
                return $"{type} {level} actor";
            }
        }

        public string MoveToAnotherHeader => $"move to {(this.IsSeriesRole ? "video" : "series")}";

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
            var imdb = this.ImdbItem.GetValidImdb();
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

            var jrySeries = this.ImdbItem as JrySeries;
            var cover = jrySeries != null
                ? JryCover.CreateRole(jrySeries, url, this.Source)
                : JryCover.CreateRole((JryVideoInfo)this.ImdbItem, url, this.Source);
            var guid = await JryVideoCore.Current.CurrentDataCenter.CoverManager.DownloadCoverAsync(cover);
            if (guid != null)
            {
                this.Source.CoverId = guid;
                await this.parent.CommitAsync(this.ImdbItem.Id);
                return true;
            }
            return false;
        }

        public async void BeginDelete() => await this.parent.DeleteAsync(this);

        public async void BegionMoveToAnotherCollection()
        {
            var dest = await this.parent.MoveToAnotherCollectionAsync(this);
            if (dest != null)
            {
                this.ImdbItem = dest;
                this.parent.Roles.Collection.Remove(this);
                this.parent.Roles.Collection.Add(this);
            }
        }

        public async void BeginEdit(Window parent)
        {
            if (RoleEditorWindow.Edit(parent, this.Source))
            {
                this.RefreshProperties();
                await this.parent.CommitAsync(this.ImdbItem.Id);
            }
        }
    }
}