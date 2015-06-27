using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using JryVideo.Core.Douban;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class CoverManager : JryObjectManager<JryCover, ICoverDataSourceProvider>
    {
        private readonly object _syncRoot = new object();
        private readonly List<string> _writingDoubanId = new List<string>();
        private readonly Dictionary<string, JryCover> Cache;

        public CoverManager(ICoverDataSourceProvider source)
            : base(source)
        {
            this.Cache = new Dictionary<string, JryCover>();
        }

        public async Task<JryCover> LoadCoverAsync(string coverId)
        {
            if (coverId == null) return null;

            return await Task.Run(async() =>
            {
                JryCover cover;

                if (this.Cache.TryGetValue(coverId, out cover))
                    return cover;

                cover = await this.Source.FindAsync(coverId);

                if (cover != null)
                {
                    lock (this.Cache)
                    {
                        return this.Cache.GetOrSetValue(coverId, cover);
                    }
                }

                return null;
            });
        }

        public async Task<string> GetCoverFromDoubanIdAsync(JryCoverType type, string doubanId)
        {
            if (String.IsNullOrWhiteSpace(doubanId))
                throw new ArgumentException();

            return await Task.Run(async () =>
            {
                foreach (var jryCover in await this.Source.QueryByDoubanIdAsync(type, doubanId))
                {
                    return jryCover.Id;
                }

                var url = await this.TryGetCoverUrlFromDoubanIdAsync(type, doubanId);

                if (url == null) return null;

                lock (this._syncRoot)
                {
                    if (this._writingDoubanId.Contains(doubanId)) return null;

                    this._writingDoubanId.Add(doubanId);
                }

                var request = WebRequest.CreateHttp(url);

                var result = await request.GetResultAsBytesAsync();

                if (result.IsSuccess)
                {
                    var cover = new JryCover();
                    cover.BuildMetaData();
                    cover.CoverSourceType = JryCoverSourceType.Douban;
                    cover.CoverType = JryCoverType.Video;
                    cover.DoubanId = doubanId;
                    cover.Uri = url;
                    cover.BinaryData = result.Result;
                    await this.InsertAsync(cover);

                    lock (this._syncRoot)
                    {
                        this._writingDoubanId.Remove(doubanId);
                    }

                    return cover.Id;
                }
                else
                {
                    lock (this._syncRoot)
                    {
                        this._writingDoubanId.Remove(doubanId);
                    }

                    return null;
                }
            });
        }

        private async Task<string> TryGetCoverUrlFromDoubanIdAsync(JryCoverType type, string doubanId)
        {
            DoubanEntity json = null;

            switch (type)
            {
                case JryCoverType.Video:
                    json = await DoubanHelper.TryGetMovieInfoAsync(doubanId);
                    break;

                case JryCoverType.Artist:
                    json = await DoubanHelper.TryGetArtistInfoAsync(doubanId);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }


            return json != null ? json.GetLargeImageUrl() : null;
        }
    }
}