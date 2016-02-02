using JryVideo.Data.DataSources;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

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

        public void SeriesManager_SeriesCreated(object sender, JrySeries e)
        {
            throw new NotImplementedException();
        }

        public async void SeriesManager_VideoInfoCreated(object sender, IEnumerable<JryVideoInfo> e)
        {
            foreach (var video in e)
            {
                await this.AutoCreateVideoRoleOnInitialize(video);
            }
        }

        public async Task AutoCreateVideoRoleOnInitialize(IImdbItem item)
        {
            await Task.Run(async () =>
            {
                await this.AutoCreateVideoRoleOnInitializeAsync(item.Id, item.GetValidImdbId());
            });
        }

        private async Task AutoCreateVideoRoleOnInitializeAsync(string id, string imdb)
        {
            var collection = await this.FindAsync(id);
            if (collection.MajorRoles != null || collection.MinorRoles != null) return;

            var client = JryVideoCore.Current.TheTVDBClient;
            if (client == null) return;

            if (imdb == null) return;
            var series = (await client.GetSeriesByImdbIdAsync(imdb)).FirstOrDefault();
            if (series == null) return;
            var actors = (await series.GetActorsAsync(client)).ToArray();
            if (actors.Length == 0) return;
            var major = actors.Select(z => z.SortOrder).Min();

            collection = await this.FindAsync(id); // sure collection was newest.
            if (collection.MajorRoles != null || collection.MinorRoles != null) return;

            foreach (var actor in actors)
            {
                var artist = (await this.artistManager.Source.FindAsync(new JryArtist.QueryParameter()
                {
                    TheTVDBId = actor.Id
                })).FirstOrDefault();

                if (artist == null && !actor.Name.IsNullOrWhiteSpace())
                {
                    artist = new JryArtist()
                    {
                        TheTVDBId = actor.Id,
                        Names = new List<string>() { actor.Name.Trim() }
                    };
                    artist.BuildMetaData();
                    await this.artistManager.InsertAsync(artist);
                }

                if (artist != null)
                {
                    var role = new JryVideoRole()
                    {
                        Id = artist.Id,
                    };
                    if (!actor.Role.IsNullOrWhiteSpace())
                    {
                        role.RoleName = new List<string>() { actor.Role.Trim() };
                    }
                    role.BuildMetaData(true);
                    (actor.SortOrder == major
                        ? (collection.MajorRoles ?? (collection.MajorRoles = new List<JryVideoRole>()))
                        : (collection.MinorRoles ?? (collection.MinorRoles = new List<JryVideoRole>()))).Add(role);
                }
            }
            await this.UpdateAsync(collection);
        }

        public async Task<IEnumerable<Tuple<VideoRoleCollection, JryVideoRole>>> QueryByActorIdAsync(string id)
        {
            return (await this.Source.FindAsync(new VideoRoleCollection.QueryParameter()
            {
                ActorId = id
            })).SelectMany(z => MatchActorId(id, z)).ToArray();
        }

        private static IEnumerable<Tuple<VideoRoleCollection, JryVideoRole>> MatchActorId(string id, VideoRoleCollection collection)
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
    }
}