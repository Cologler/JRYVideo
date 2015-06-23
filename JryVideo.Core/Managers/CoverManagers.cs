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

        public CoverManager(ICoverDataSourceProvider source)
            : base(source)
        {
        }

        public async Task<JryCover> LoadCoverAsync(string coverId)
        {
            return coverId == null ? null : await this.Source.QueryAsync(coverId);
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