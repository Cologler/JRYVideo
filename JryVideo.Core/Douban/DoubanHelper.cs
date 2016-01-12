using Jasily.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Jasily.SDK.Douban.Entities;

namespace JryVideo.Core.Douban
{
    public static class DoubanHelper
    {
        private const string ApiUrl = "http://api.douban.com/v2/movie/subject/";

        public static async Task<Movie> TryGetMovieInfoAsync(string doubanId)
        {
            var request = WebRequest.CreateHttp("http://api.douban.com/v2/movie/subject/" + doubanId);

            var result = await request.GetResultAsBytesAsync();

            if (result.IsSuccess)
            {
                try
                {
                    return result.AsJson<Movie>().Result;
                }
                catch
                {
                    // ignored
                }
            }

            return null;
        }

        public static async Task<Artist> TryGetArtistInfoAsync(string doubanId)
        {
            var request = WebRequest.CreateHttp("http://api.douban.com/v2/movie/celebrity/" + doubanId);

            var result = await request.GetResultAsBytesAsync();

            if (result.IsSuccess)
            {
                try
                {
                    return result.AsJson<Artist>().Result;
                }
                catch
                {
                    // ignored
                }
            }

            return null;
        }

        public static string GetLargeImageUrl(this Artist json)
        {
            return json.ThrowIfNull("json").Images.Large.ThrowIfNullOrEmpty("Large");
        }

        public static string GetLargeImageUrl(this Movie json)
        {
            return json.ThrowIfNull("json").Images.Large.ThrowIfNullOrEmpty("Large");
        }

        public static string GetRawImageUrl(Movie json)
        {
            var large = GetLargeImageUrl(json);
            // large like 'http://img4.douban.com/view/movie_poster_cover/ipst/public/p2236401229.jpg'

            var server = large[10].ToString();
            var item = large.Substring(large.LastIndexOf('/'));
            return String.Format(@"http://img{0}.douban.com/view/photo/raw/public{1}", server, item);
        }

        public static IEnumerable<string> ParseName(Artist json)
        {
            if (!String.IsNullOrWhiteSpace(json.Name))
                yield return json.Name;

            if (!String.IsNullOrWhiteSpace(json.OriginName))
                yield return json.OriginName;

            if (json.OtherNames != null)
            {
                foreach (var name in json.OtherNames.Where(z => !String.IsNullOrWhiteSpace(z)))
                {
                    yield return name;
                }
            }

            if (json.OtherOriginNames != null)
            {
                foreach (var name in json.OtherOriginNames.Where(z => !String.IsNullOrWhiteSpace(z)))
                {
                    yield return name;
                }
            }
        }

        public static IEnumerable<string> ParseName(this Movie json)
        {
            if (!String.IsNullOrWhiteSpace(json.Title))
                yield return json.Title;

            if (!String.IsNullOrWhiteSpace(json.OriginalTitle))
                yield return json.OriginalTitle;

            if (json.OtherNames != null)
            {
                foreach (var originName in json.OtherNames.Where(z => !String.IsNullOrWhiteSpace(z)))
                {
                    if (originName.EndsWith("(港)") || originName.EndsWith("(台)"))
                        yield return originName.Substring(0, originName.Length - 3);
                    else
                        yield return originName;
                }
            }
        }
    }
}