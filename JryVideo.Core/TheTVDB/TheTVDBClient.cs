using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Jasily.Net;
using JryVideo.Model;

namespace JryVideo.Core.TheTVDB
{
    public class TheTVDBClient
    {
        private readonly string apiKey;
        private readonly List<Mirror> allMirror;
        private readonly List<Mirror> xmlMirror = new List<Mirror>();
        private readonly List<Mirror> bannerMirror = new List<Mirror>();
        private readonly List<Mirror> zipMirror = new List<Mirror>();

        internal TheTVDBClient(string apiKey, IEnumerable<Mirror> mirrors)
        {
            this.apiKey = apiKey;
            this.allMirror = mirrors.ToList();
            foreach (var mirror in this.allMirror)
            {
                if ((mirror.Type & MirrorType.Xml) == MirrorType.Xml)
                    this.xmlMirror.Add(mirror);
                if ((mirror.Type & MirrorType.Banner) == MirrorType.Banner)
                    this.bannerMirror.Add(mirror);
                if ((mirror.Type & MirrorType.Zip) == MirrorType.Zip)
                    this.zipMirror.Add(mirror);
            }
        }

        public async Task<IEnumerable<Series>> GetSeriesByImdbIdAsync(string imdbId)
        {
            if (imdbId == null) throw new ArgumentNullException(nameof(imdbId));
            if (!imdbId.StartsWith("tt")) throw new ArgumentException(nameof(imdbId));
            if (this.allMirror.Count == 0) throw new NotSupportedException();

            var url = $"{this.allMirror.RandomTake(new Random()).MirrorPath}/api/GetSeriesByRemoteID.php?imdbid={imdbId}";
            var request = WebRequest.CreateHttp(url);
            var result = (await request.GetResultAsBytesAsync()).AsXml<SeriesArray>();
            if (result.IsSuccess && result.Result?.Series != null) return result.Result.Series;
            return Enumerable.Empty<Series>();
        }

        public async Task<byte[]> GetBannerByBannerPathAsync(string banner)
        {
            var url = this.BuildBannerUrl(banner);
            var request = WebRequest.CreateHttp(url);
            var result = await request.GetResultAsBytesAsync();
            return result.IsSuccess ? result.Result : null;
        }

        public string BuildBannerUrl(string banner)
        {
            if (banner == null) throw new ArgumentNullException(nameof(banner));
            if (this.bannerMirror.Count == 0) throw new NotSupportedException();

            return $"{this.bannerMirror.RandomTake(new Random()).MirrorPath}/banners/{banner}";
        }

        public async Task<IEnumerable<Banner>> GetBannersBySeriesIdAsync(string seriesId)
        {
            if (seriesId == null) throw new ArgumentNullException(nameof(seriesId));

            var url = $"{this.allMirror.RandomTake(new Random()).MirrorPath}/api/{this.apiKey}/series/{seriesId}/banners.xml";
            var request = WebRequest.CreateHttp(url);
            var result = (await request.GetResultAsBytesAsync()).AsXml<BannerArray>();
            return result.IsSuccess && result.Result.Banners != null
                ? result.Result.Banners
                : Enumerable.Empty<Banner>();
        }

        public async Task<IEnumerable<Actor>> GetActorsBySeriesIdAsync(string seriesId)
        {
            if (seriesId == null) throw new ArgumentNullException(nameof(seriesId));

            var url = $"{this.allMirror.RandomTake(new Random()).MirrorPath}/api/{this.apiKey}/series/{seriesId}/actors.xml";
            var request = WebRequest.CreateHttp(url);
            var result = (await request.GetResultAsBytesAsync()).AsXml<ActorArray>();
            return result.IsSuccess && result.Result.Actors != null
                ? result.Result.Actors
                : Enumerable.Empty<Actor>();
        }

        public async Task<string> TryGetTheTVDBSeriesIdByRemoteIdAsync(RemoteId id)
        {
            switch (id.Type)
            {
                case RemoteIdType.TheTVDB:
                    return id.Id;

                case RemoteIdType.Imdb:
                    return (await this.GetSeriesByImdbIdAsync(id.Id)).FirstOrDefault()?.SeriesId;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}