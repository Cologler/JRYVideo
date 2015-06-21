using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace JryVideo.Core.Douban
{
    public class DoubanHelper
    {
        private const string ApiUrl = "http://api.douban.com/v2/movie/subject/";

        public static async Task<DoubanMovieJson> GetMovieInfoAsync(string doubanId)
        {
            var request = WebRequest.CreateHttp("http://api.douban.com/v2/movie/subject/" + doubanId);

            var result = await request.GetResultAsBytesAsync();

            if (result.IsSuccess)
            {
                try
                {
                    return result.AsJson<DoubanMovieJson>().Result;
                }
                catch
                {
                    // ignored
                }
            }

            return null;
        }

        public static string GetLargeImageUrl(DoubanMovieJson json)
        {
            return json.ThrowIfNull("json").Images.Large.ThrowIfNullOrEmpty("Large");
        }

        public static string GetRawImageUrl(DoubanMovieJson json)
        {
            var large = GetLargeImageUrl(json);
            // large like 'http://img4.douban.com/view/movie_poster_cover/ipst/public/p2236401229.jpg'

            var server = large[10].ToString();
            var item = large.Substring(large.LastIndexOf('/'));
            return String.Format(@"http://img{0}.douban.com/view/photo/raw/public{1}", server, item);
        }

        public static IEnumerable<string> ParseName(DoubanMovieJson json)
        {
            if (!String.IsNullOrWhiteSpace(json.OriginalTitle))
                yield return json.OriginalTitle;

            if (!String.IsNullOrWhiteSpace(json.Title))
                yield return json.Title;

            if (json.Aka != null)
            {
                foreach (var originName in json.Aka.Where(z => !String.IsNullOrWhiteSpace(z)))
                {
                    yield return originName;
                }
            }
        }
    }
}