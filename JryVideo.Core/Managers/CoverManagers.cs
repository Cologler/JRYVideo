using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Jasily.Net;
using JryVideo.Core.Douban;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class CoverManager : JryObjectManager<JryCover, ICoverSet>, IJasilyLoggerObject<CoverManager>
    {
        private readonly object _syncRoot = new object();
        private readonly List<string> _writingDoubanId = new List<string>();
        private readonly MemoryCache MemoryCache;

        public CoverManager(ICoverSet source)
            : base(source)
        {
            this.MemoryCache = MemoryCache.Default;
        }

        public async Task<JryCover> LoadCoverAsync(string coverId)
        {
            if (coverId == null) return null;

            return await Task.Run(async() =>
            {
                JryCover cover;

                var obj = this.MemoryCache.Get(coverId);
                if (obj != null) return (JryCover)obj;

                cover = await this.Source.FindAsync(coverId);

                if (cover != null)
                {
                    lock (this.MemoryCache)
                    {
                        obj = this.MemoryCache.AddOrGetExisting(coverId, cover, DateTimeOffset.UtcNow.AddHours(1));
                        return obj != null ? (JryCover) obj : cover;
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
                    this.Log(JasilyLogger.LoggerMode.Debug, String.Format("added thread for cover. total [{0}].", this._writingDoubanId.Count));
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
                        this.Log(JasilyLogger.LoggerMode.Debug, String.Format("finish thread for cover. total [{0}].", this._writingDoubanId.Count));
                    }

                    return null;
                }
            });
        }

        private async Task<string> TryGetCoverUrlFromDoubanIdAsync(JryCoverType type, string doubanId)
        {
            Jasily.SDK.Douban.Entities.BaseEntity json = null;

            switch (type)
            {
                case JryCoverType.Video:
                    return (await DoubanHelper.TryGetMovieInfoAsync(doubanId))?.GetLargeImageUrl();

                case JryCoverType.Artist:
                    return (await DoubanHelper.TryGetArtistInfoAsync(doubanId))?.GetLargeImageUrl();

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public override async Task<bool> UpdateAsync(JryCover obj)
        {
            var result = await base.UpdateAsync(obj);
            this.MemoryCache.Remove(obj.Id);
            return result;
        }

        public override async Task<bool> RemoveAsync(string id)
        {
            var result = await base.RemoveAsync(id);
            this.MemoryCache.Remove(id);
            return result;
        }
    }
}