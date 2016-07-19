using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JryVideo.Core;
using JryVideo.Core.Managers;
using JryVideo.Core.Models;
using JryVideo.Core.TheTVDB;
using JryVideo.Editors.RoleEditor;
using JryVideo.Model;
using JryVideo.Model.Interfaces;
using Series = JryVideo.Model.Series;

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

            this.CoverViewModel.AutoGenerateCoverProvider = new AutoGenerateCoverProvider()
            {
                ImdbItem = imdbItem,
                TheTVDBItem = this.ImdbItem as ITheTVDBItem
            };
        }

        public IImdbItem ImdbItem { get; private set; }

        public bool IsSeriesRole => this.ImdbItem is Series;

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

        private class AutoGenerateCoverProvider : IAutoGenerateCoverProvider
        {
            public IImdbItem ImdbItem { get; set; }

            public ITheTVDBItem TheTVDBItem { get; set; }

            /// <summary>
            /// return true if success.
            /// </summary>
            /// <returns></returns>
            public async Task<bool> GenerateAsync(DataCenter dataCenter, ICoverParent source)
            {
                var client = JryVideoCore.Current.GetTheTVDBClient();
                if (client == null) return false;

                var role = (VideoRole)source;

                var tvdb = this.TheTVDBItem;
                if (tvdb != null && !tvdb.TheTVDBId.IsNullOrWhiteSpace())
                {
                    if (await this.TryAutoAddCoverAsync(dataCenter, client, new RemoteId(RemoteIdType.TheTVDB, tvdb.TheTVDBId), role))
                    {
                        return true;
                    }
                }

                var imdb = this.ImdbItem.GetValidImdbId();
                if (imdb != null)
                {
                    if (await this.TryAutoAddCoverAsync(dataCenter, client, new RemoteId(RemoteIdType.Imdb, imdb), role))
                    {
                        return true;
                    }
                }

                return false;
            }

            private async Task<bool> TryAutoAddCoverAsync(DataCenter dataCenter, TheTVDBClient client, RemoteId removeId, VideoRole role)
            {
                var theTVDBId = await client.TryGetTheTVDBSeriesIdByRemoteIdAsync(removeId);
                if (theTVDBId == null) return false;
                var actor = await dataCenter.ArtistManager.FindAsync(role.ActorId);
                if (actor == null) return false;
                var actors = (await client.GetActorsBySeriesIdAsync(theTVDBId)).ToArray();
                actors = actors.Where(z => z.Id == actor.TheTVDBId).ToArray();
                if (actors.Length != 1) return false;
                if (!actors[0].HasBanner) return false;
                var url = actors[0].BuildUrl(client);
                var builder = CoverBuilder.CreateRole(role);
                builder.Uri.Add(url);
                return await dataCenter.CoverManager.BuildCoverAsync(builder);
            }
        }
    }
}