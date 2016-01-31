using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public sealed class VideoRoleManager :
        AutoInsertVideoInfoAttachedManager<VideoRoleCollection, IJasilyEntitySetProvider<VideoRoleCollection, string>>
    {
        private readonly ArtistManager artistManager;
        private readonly SeriesManager seriesManager;

        public VideoRoleManager(SeriesManager seriesManager, ArtistManager artistManager, IJasilyEntitySetProvider<VideoRoleCollection, string> source)
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
                await this.AutoCreateVideoRoleOnInitializeAsync(item.Id, item.GetValidImdb());
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
                        ActorName = artist.Names.First()
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
    }
}