using Jasily.Net;
using System;
using System.Net;
using System.Threading.Tasks;

namespace JryVideo.Core.TheTVDB
{
    public class TheTVDBHost
    {
        public async Task<TheTVDBClient> CreateAsync(string apiKey, string secretKey = null)
        {
            if (apiKey.IsNullOrWhiteSpace()) return null;

            var request = WebRequest.CreateHttp($"https://thetvdb.com/api/{apiKey}/mirrors.xml");
            var result = (await request.GetResultAsBytesAsync()).AsXml<MirrorArray>();

            if (result.IsSuccess)
            {
                var mirrors = result.Result.Mirrors;
                if (mirrors != null && mirrors.Length > 0)
                {
                    return new TheTVDBClient(apiKey, mirrors);
                }
            }
            return null;
        }
    }
}