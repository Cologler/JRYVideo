using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace JryVideo.Core.Douban
{
    public class DoubanHelper
    {
        private const string ApiUrl = "http://api.douban.com/v2/movie/subject/";

        public static async Task<DoubanMovieJson> SendAsync(string doubanId)
        {
            if (String.IsNullOrWhiteSpace(doubanId)) return null;

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


    }
}