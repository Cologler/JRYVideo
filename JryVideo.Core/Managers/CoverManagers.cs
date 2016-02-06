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

            using (var start = this.StartDownload(cover.GetDownloadId()))
            {
                if (!start.IsOwner) return null;

                return await Task.Run(async () =>
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

                    return null;
                });
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

        public async void Manager_CoverParentRemoving(object sender, IJryCoverParent e)
        {
            var id = e.CoverId;
            if (id != null)
            {
                Debug.WriteLine($"remove cover [{id}] from {e.GetType().Name}[ {e.Id}] over {sender.GetType().Name}");
                await this.RemoveAsync(id);
            }
        }

        private DownloadProcess StartDownload(string id) => new DownloadProcess(id);

        private class DownloadProcess : IDisposable
        {
            private readonly string id;
            private static readonly Dictionary<string, bool> Processs = new Dictionary<string, bool>();

            public DownloadProcess(string id)
            {
                this.id = id;
                lock (Processs)
                {
                    this.IsOwner = !Processs.ContainsKey(id);
                    if (this.IsOwner)
                    {
                        Processs.Add(id, false);
                    }
                }
            }

            public bool IsOwner { get; }

            public void Dispose()
            {
                lock (Processs)
                {
                    if (this.IsOwner)
                    {
                        Processs.Remove(this.id);
                    }
                }
            }
        }
    }
}