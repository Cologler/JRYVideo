using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JryVideo.Core;
using JryVideo.Core.Models;
using JryVideo.Core.TheTVDB;
using JryVideo.Editors.RoleEditor;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class VideoRoleViewModel : VideoRoleReadonlyViewModel
    {
        private readonly VideoRoleCollectionViewModel parent;

        public VideoRoleViewModel(VideoRole source, VideoRoleCollectionViewModel parent, IImdbItem imdbItem, bool isMajor)
            : base(source)
        {
            this.parent = parent;
            this.ImdbItem = imdbItem;
            this.IsMajor = isMajor;
        }

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
            var client = JryVideoCore.Current.GetTheTVDBClient();
            if (client == null) return false;

            var theTVDBItem = this.ImdbItem as ITheTVDBItem;
            if (theTVDBItem != null &&
                !theTVDBItem.TheTVDBId.IsNullOrWhiteSpace() &&
                await this.TryAutoAddCoverAsync(client, new RemoteId(RemoteIdType.TheTVDB, theTVDBItem.TheTVDBId)))
            {
                return true;
            }

            var imdb = this.ImdbItem.GetValidImdbId();
            if (imdb != null &&
                await this.TryAutoAddCoverAsync(client, new RemoteId(RemoteIdType.Imdb, imdb)))
            {
                return true;
            }

            return false;
        }

        private async Task<bool> TryAutoAddCoverAsync(TheTVDBClient client, RemoteId removeId)
        {
            var theTVDBId = await client.TryGetTheTVDBSeriesIdByRemoteIdAsync(removeId);
            if (theTVDBId == null) return false;
            var artist = await this.GetManagers().ArtistManager.FindAsync(this.Source.ActorId);
            if (artist == null) return false;
            var actors = (await client.GetActorsBySeriesIdAsync(theTVDBId)).ToArray();
            actors = actors.Where(z => z.Id == artist.TheTVDBId)
                .ToArray();
            if (actors.Length != 1) return false;
            if (!actors[0].HasBanner) return false;
            var url = actors[0].BuildUrl(client);

            var jrySeries = this.ImdbItem as JrySeries;
            var builder = jrySeries != null
                ? CoverBuilder.CreateRole(jrySeries, url, this.Source)
                : CoverBuilder.CreateRole((JryVideoInfo)this.ImdbItem, url, this.Source);
            var guid = await this.GetManagers().CoverManager.BuildCoverAsync(builder);
            if (guid != null)
            {
                await this.parent.CommitAsync();
                return true;
            }
            return false;
        }

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
                await this.parent.CommitAsync();
            }
        }
    }
}