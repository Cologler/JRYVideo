using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Jasily.Net;
using JryVideo.Core.Models;
using JryVideo.Data.DataSources;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

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

        public async Task<string> BuildCoverAsync(CoverBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            using (var start = new DownloadProcess(builder.CustomId ?? builder.BuildDownloadId()))
            {
                if (!start.IsOwner) return null;

                return await Task.Run(async () =>
                {
                    var covers = (await this.Source.FindAsync(builder.BuildQueryParameter())).ToArray();
                    try
                    {
                        var cover = covers.SingleOrDefault();
                        if (cover != null) return cover.Id;
                    }
                    catch (InvalidOperationException)
                    {
                        Log.Write($"series [{builder.SeriesId}] video [{builder.VideoId}] cover [{builder.CoverType}] has more then 1 result.");
                        return null;
                    }
                    if (string.IsNullOrWhiteSpace(builder.Uri)) return null;

                    HttpWebRequest request;
                    try { request = WebRequest.CreateHttp(builder.Uri); }
                    catch { return null; }
                    var result = await request.GetResultAsBytesAsync();
                    if (result.IsSuccess)
                    {
                        var cover = builder.Build(result.Result);
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