using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Jasily.Net;
using JryVideo.Core.Managers.Upgrades;
using JryVideo.Core.Models;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Managers
{
    public class CoverManager : JryObjectManager<JryCover, ICoverSet>, IJasilyLoggerObject<CoverManager>
    {
        private readonly MemoryCache MemoryCache;
        private readonly Dictionary<string, Task<bool>> downloaders = new Dictionary<string, Task<bool>>();

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

        public Task<bool> BuildCoverAsync(CoverBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            Task<bool> task;
            lock (this.downloaders)
            {
                task = this.downloaders.GetValueOrDefault(builder.Id);
                if (task == null)
                {
                    if (builder.Requests.Count + builder.Uri.Count == 0) return Task.FromResult(false);
                    task = Task.Run(async () =>
                    {
                        foreach (var request in builder.Requests.Concat(builder.Uri.Select(z =>
                        {
                            try { return WebRequest.CreateHttp(z); }
                            catch { return null; }
                        }).Where(z => z != null)))
                        {
                            var result = await request.GetResultAsBytesAsync();
                            if (!result.IsSuccess) continue;
                            var cover = builder.Build(result.Result);
                            cover.Version = Upgrader<JryCover>.MaxVersion;
                            if (await this.InsertOrUpdateAsync(cover)) return true;
                        }
                        return false;
                    });
                    this.downloaders[builder.Id] = task;
                }
            }
            return task;
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

        private async void Manager_CoverParentRemoving(object sender, ICoverParent e)
        {
            var id = e.CoverId;
            if (id != null)
            {
                Debug.WriteLine($"remove cover [{id}] from {e.GetType().Name}[ {e.Id}] over {sender.GetType().Name}");
                await this.RemoveAsync(id);
            }
        }

        private struct DownloadProcess : IDisposable
        {
            private readonly string id;
            private static readonly HashSet<string> Processs = new HashSet<string>();

            public DownloadProcess(string id)
            {
                this.id = id;
                lock (Processs)
                {
                    this.IsOwner = !Processs.Contains(id);
                    if (this.IsOwner)
                    {
                        Processs.Add(id);
                    }
                }
            }

            public bool IsOwner { get; }

            public void Dispose()
            {
                if (this.IsOwner)
                {
                    lock (Processs)
                    {
                        Processs.Remove(this.id);
                    }
                }
            }
        }

        public void Initialize(DataCenter dataCenter)
        {
            dataCenter.SeriesManager.CoverParentRemoving += this.Manager_CoverParentRemoving;
            dataCenter.VideoRoleManager.CoverParentRemoving += this.Manager_CoverParentRemoving;
            dataCenter.ArtistManager.CoverParentRemoving += this.Manager_CoverParentRemoving;
        }
    }
}