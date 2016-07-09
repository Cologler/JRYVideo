using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public sealed class VideoRoleManager :
        AutoInsertVideoInfoAttachedManager<VideoRoleCollection, IVideoRoleCollectionSet>
    {
        private readonly ArtistManager artistManager;
        private readonly SeriesManager seriesManager;

        public VideoRoleManager(SeriesManager seriesManager, ArtistManager artistManager, IVideoRoleCollectionSet source)
            : base(source)
        {
            this.seriesManager = seriesManager;
            this.artistManager = artistManager;
        }

        public void Initialize(DataCenter dataCenter)
        {
            dataCenter.SeriesManager.ItemRemoved += this.SeriesManager_ItemRemoved;
            dataCenter.SeriesManager.VideoInfoRemoved += this.SeriesManager_VideoInfoRemoved;
        }

        public async Task<bool> AutoCreateVideoRoleAsync(string id, RemoteId remoteId)
        {
            var collection = await this.FindAsync(id);
            if (collection.MajorRoles != null || collection.MinorRoles != null) return false;

            var client = JryVideoCore.Current.TheTVDBHost.LastClientInstance;
            if (client == null) return false;

            var theTVDBId = await client.TryGetTheTVDBSeriesIdByRemoteIdAsync(remoteId);
            if (theTVDBId == null) return false;

            var actors = (await client.GetActorsBySeriesIdAsync(theTVDBId)).ToArray();
            if (actors.Length == 0) return false;
            var major = actors.Select(z => z.SortOrder).Min();

            collection = await this.FindAsync(id); // sure collection was newest.
            if (collection.MajorRoles != null || collection.MinorRoles != null) return false;

            foreach (var actor in actors)
            {
                var artist = (await this.artistManager.Source.FindAsync(new Artist.QueryParameter()
                {
                    TheTVDBId = actor.Id
                })).FirstOrDefault();

                if (artist == null && !actor.Name.IsNullOrWhiteSpace())
                {
                    artist = new Artist()
                    {
                        TheTVDBId = actor.Id,
                        Names = new List<string>() { actor.Name.Trim() }
                    };
                    artist.BuildMetaData();
                    await this.artistManager.InsertAsync(artist);
                }

                if (artist != null)
                {
                    var role = new VideoRole()
                    {
                        Id = artist.Id,
                    };
                    if (!actor.Role.IsNullOrWhiteSpace())
                    {
                        role.RoleName = new List<string>() { actor.Role.Trim() };
                    }
                    role.BuildMetaData(true);
                    (actor.SortOrder == major
                        ? (collection.MajorRoles ?? (collection.MajorRoles = new List<VideoRole>()))
                        : (collection.MinorRoles ?? (collection.MinorRoles = new List<VideoRole>()))).Add(role);
                }
            }
            await this.UpdateAsync(collection);
            return true;
        }

        public async Task<IEnumerable<Tuple<VideoRoleCollection, VideoRole>>> QueryByActorIdAsync(string id)
        {
            return (await this.Source.FindAsync(new VideoRoleCollection.QueryParameter()
            {
                ActorId = id
            })).SelectMany(z => MatchActorId(id, z)).ToArray();
        }

        private static IEnumerable<Tuple<VideoRoleCollection, VideoRole>> MatchActorId(string id, VideoRoleCollection collection)
        {
            if (collection.MajorRoles != null)
            {
                foreach (var role in collection.MajorRoles.Where(role => role.ArtistId == id))
                {
                    yield return Tuple.Create(collection, role);
                }
            }
            if (collection.MinorRoles != null)
            {
                foreach (var role in collection.MinorRoles.Where(role => role.ArtistId == id))
                {
                    yield return Tuple.Create(collection, role);
                }
            }
        }

        public override async Task<bool> RemoveAsync(string id)
        {
            var item = await this.FindAsync(id);
            if (item != null)
            {
                if (item.MajorRoles != null)
                {
                    foreach (var role in item.MajorRoles)
                    {
                        this.OnCoverParentRemoving(role);
                    }
                }
                if (item.MinorRoles != null)
                {
                    foreach (var role in item.MinorRoles)
                    {
                        this.OnCoverParentRemoving(role);
                    }
                }
            }
            return await base.RemoveAsync(id);
        }

        private async void SeriesManager_VideoInfoRemoved(object sender, IEnumerable<JryVideoInfo> e)
        {
            foreach (var info in e)
            {
                await this.RemoveAsync(info.Id);
            }
        }

        internal override Task<CombineResult> CanCombineAsync(VideoRoleCollection to, VideoRoleCollection @from)
            => Task.FromResult(CombineResult.True);

        internal override async Task<CombineResult> CombineAsync(VideoRoleCollection to, VideoRoleCollection @from)
        {
            var result = await this.CanCombineAsync(to, from);
            if (result.CanCombine)
            {
                to.CombineFrom(@from);
                await this.UpdateAsync(to);
            }
            return result;
        }

        private async void SeriesManager_ItemRemoved(object sender, string e) => await this.RemoveAsync(e);
    }
}