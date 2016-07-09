using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Jasily.Net;
using Jasily.SDK.Douban.Entities;

namespace JryVideo.Core.Douban
{
    public static class DoubanHelper
    {
        public static async Task<Movie> TryGetMovieInfoAsync(string doubanId)
        {
            var request = WebRequest.CreateHttp("http://api.douban.com/v2/movie/subject/" + doubanId);
            using (var result = await request.GetResultAsBytesAsync())
            {
                PrintInDebug(result);
                return result.TryAsJson<Movie>().Result;
            }
        }

        public static async Task<string> TryGetImdbIdAsync(string doubanId)
        {
            var request = WebRequest.CreateHttp("https://movie.douban.com/subject/" + doubanId);
            using (var result = await request.GetResultAsBytesAsync().AsText())
            {
                if (result.Result == null) return null;
                var match = Regex.Match(result.Result, "href=\"http://www.imdb.com/title/(tt\\d*)\"");
                return match.Success ? match.Groups[1].Value : null;
            }
        }

        public static async Task<string> TryGetMovieHtmlAsync(string doubanId)
        {
            var request = WebRequest.CreateHttp("https://movie.douban.com/subject/" + doubanId);
            using (var result = await request.GetResultAsBytesAsync().AsText())
            {
                return result.Result;
            }
        }

        public static string TryParseImdbId(string html)
        {
            Debug.Assert(html != null);
            var match = Regex.Match(html, "href=\"http://www.imdb.com/title/(tt\\d*)\"");
            return match.Success ? match.Groups[1].Value : null;
        }

        public static DateTime? TryParseReleaseDate(string html)
        {
            Debug.Assert(html != null);
            var str = TryParseReleaseDateString(html);
            if (str == null) return null;
            var match = Regex.Match(str, "(\\d{4})-(\\d{2})-(\\d{2})");
            var year = int.Parse(match.Groups[1].Value);
            var month = int.Parse(match.Groups[2].Value);
            var day = int.Parse(match.Groups[3].Value);
            return DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Local);
        }

        private static string TryParseReleaseDateString(string html)
        {
            Debug.Assert(html != null);
            var matchs = Regex.Matches(html, "property=\"v:initialReleaseDate\" content=\"(\\d{4}-\\d{2}-\\d{2})([^\"]*)?\"");
            if (matchs.Count == 1)
            {
                return matchs[0].Groups[1].Value;
            }
            else
            {
                var f = 0;
                string value = null;
                foreach (Match match in matchs)
                {
                    if (match.Groups.Count == 1) return match.Groups[1].Value;

                    int fx;
                    switch (match.Groups[2].Value)
                    {
                        case "(美国)":
                            fx = 10;
                            break;
                        case "(日本)":
                            fx = 9;
                            break;
                        default:
                            fx = 1;
                            break;
                    }

                    if (f < fx)
                    {
                        f = fx;
                        value = match.Groups[1].Value;
                    }
                }
                return value;
            }
            return null;
        }

        public static async Task<Artist> TryGetArtistInfoAsync(string doubanId)
        {
            var request = WebRequest.CreateHttp("http://api.douban.com/v2/movie/celebrity/" + doubanId);
            var result = await request.GetResultAsBytesAsync();
            PrintInDebug(result);
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

        [Conditional("DEBUG")]
        private static void PrintInDebug(WebResult<byte[]> result)
        {
            if (Debugger.IsAttached && result.IsSuccess)
            {
                Debug.WriteLine(result.AsText().Result ?? string.Empty);
            }
        }

        public static string GetLargeImageUrl(this Artist json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            return json.Images.Large.ThrowIfNullOrEmpty("Large");
        }

        public static IEnumerable<HttpWebRequest> GetMovieCoverRequest(this Movie json)
        {
            var raw = GetRawImageUrl(json);
            HttpWebRequest request = null;
            try
            {
                request = WebRequest.CreateHttp(raw);
            }
            catch
            {
                // ignored
            }
            if (request != null)
            {
                request.Referer = "https://www.douban.com/";
                yield return request;
            }
            request = null;
            try
            {
                request = WebRequest.CreateHttp(GetLargeImageUrl(json));
            }
            catch
            {
                // ignored
            }
            if (request != null) yield return request;
        }

        public static string GetLargeImageUrl(this Movie json)
        {
            // http get https://img3.doubanio.com/view/photo/raw/public/p2357519332.jpg Referer:https://www.douban.com/ work !
            if (json == null) throw new ArgumentNullException(nameof(json));
            return json.Images.Large.ThrowIfNullOrEmpty("Large");
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
            if (!string.IsNullOrWhiteSpace(json.Name))
                yield return json.Name;

            if (!string.IsNullOrWhiteSpace(json.OriginName))
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