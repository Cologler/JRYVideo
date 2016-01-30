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

        public VideoRoleManager(ArtistManager artistManager, IJasilyEntitySetProvider<VideoRoleCollection, string> source)
            : base(source)
        {
            this.artistManager = artistManager;
        }

        public async void SeriesManager_VideoInfoCreated(object sender, IEnumerable<JryVideoInfo> e)
        {
            foreach (var video in e)
            {
                await this.AutoCreateVideoRoleOnInitialize(video);
            }
        }

        public async Task AutoCreateVideoRoleOnInitialize(JryVideoInfo video)
        {
            var collection = await this.FindAsync(video.Id);
            if (collection.MajorRoles != null || collection.MinorRoles != null) return;

            var client = JryVideoCore.Current.TheTVDBClient;
            if (client == null) return;

            if (video.ImdbId == null || !video.ImdbId.StartsWith("tt")) return;
            var series = (await client.GetSeriesByImdbIdAsync(video.ImdbId)).FirstOrDefault();
            if (series == null) return;
            var actors = (await series.GetActorsAsync(client)).ToArray();
            if (actors.Length == 0) return;
            var major = actors.Select(z => z.SortOrder).Min();

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