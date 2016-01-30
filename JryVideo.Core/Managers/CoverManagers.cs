using Jasily.Net;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public class CoverManager : JryObjectManager<JryCover, ICoverSet>, IJasilyLoggerObject<CoverManager>
    {
        private readonly object syncRoot = new object();
        private readonly List<string> downloadingIds = new List<string>();
        private readonly MemoryCache MemoryCache;

        public CoverManager(ICoverSet source)
            : base(source)
        {
            this.MemoryCache = MemoryCache.Default;
        }

        public async Task<JryCover> LoadCoverAsync(string coverId)
        {
            if (coverId == null) return null;

            return await Task.Run(async () =>
            {
                var obj = this.MemoryCache.Get(coverId);
                if (obj != null) return (JryCover)obj;

                var cover = await this.Source.FindAsync(coverId);

                if (cover != null)
                {
                    lock (this.MemoryCache)
                    {
                        obj = this.MemoryCache.AddOrGetExisting(coverId, cover, DateTimeOffset.UtcNow.AddHours(1));
                        return obj != null ? (JryCover)obj : cover;
                    }
                }

                return null;
            });
        }

        public async Task<string> DownloadCoverAsync(JryCover cover)
        {
            if (cover == null) throw new ArgumentNullException(nameof(cover));

            var url = cover.Uri;
            if (string.IsNullOrWhiteSpace(cover.Uri))
                throw new ArgumentException();

            var key = cover.GetDownloadId();

            return await Task.Run(async () =>
            {
                lock (this.syncRoot)
                {
                    if (this.downloadingIds.Contains(key)) return null;
                    this.downloadingIds.Add(key);
                }

                try
                {
                    var request = WebRequest.CreateHttp(url);
                    var result = await request.GetResultAsBytesAsync();
                    if (result.IsSuccess)
                    {
                        cover.BuildMetaData();
                        cover.BinaryData = result.Result;
                        if (await this.InsertAsync(cover))
                        {
                            return cover.Id;
                        }
                    }
                }
                finally
                {
                    lock (this.syncRoot)
                    {
                        this.downloadingIds.Remove(key);
                    }
                }

                return null;
            });
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