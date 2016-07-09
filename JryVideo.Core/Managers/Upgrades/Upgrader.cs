using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Core.Managers.Upgrades.Patchs;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Managers.Upgrades
{
    public class Upgrader
    {
        public static readonly List<IPatch> Patchs = new List<IPatch>();

        static Upgrader()
        {
            Patchs.Add(new Patch0000());
            Patchs.Add(new Patch0001());
            Patchs.Add(new Patch0002());
            Patchs.Add(new Patch0003());
        }

        public Upgrader(DataCenter dataCenter)
        {
            this.DataCenter = dataCenter;
        }

        public DataCenter DataCenter { get; }

        public async Task RunAsync()
        {
            await new Upgrader<JrySeries>(this.DataCenter)
                .ExecuteAsync(this.DataCenter.SeriesManager.Source, this.DataCenter.SeriesManager);
            await new Upgrader<JryCover>(this.DataCenter)
                .ExecuteAsync(this.DataCenter.CoverManager.Source, this.DataCenter.CoverManager);
            await new Upgrader<VideoRoleCollection>(this.DataCenter)
                .ExecuteAsync(this.DataCenter.VideoRoleManager.Source, this.DataCenter.VideoRoleManager);
            await new Upgrader<Artist>(this.DataCenter)
                .ExecuteAsync(this.DataCenter.ArtistManager.Source, this.DataCenter.ArtistManager);

            Debug.WriteLine("upgrade completed.");
        }

        public static int Version<T>()
        {
            return Patchs.OfType<IPatch<T>>().Count();
        }
    }

    public class Upgrader<T>
        where T : class, IObject
    {
        private static readonly IPatch<T>[] Patchs;
        private static readonly IEveryTimePatch<T>[] EveryTimePatchs; // 每次都执行 所以没有版本
        private readonly DataCenter dataCenter;

        static Upgrader()
        {
            Patchs = Upgrader.Patchs.OfType<IPatch<T>>().ToArray();
            EveryTimePatchs = Upgrader.Patchs.OfType<IEveryTimePatch<T>>().ToArray();
            MaxVersion = Math.Max(Patchs.Length - 1, 0);
        }

        public Upgrader(DataCenter dataCenter)
        {
            this.dataCenter = dataCenter;
        }

        public static int MaxVersion { get; }

        public async Task ExecuteAsync(IJasilyEntitySetReader<T, string> reader, IObjectEditProvider<T> provider)
        {
            if (this.dataCenter == null) throw new NotSupportedException();

            foreach (var patch in EveryTimePatchs)
            {
                await patch.ExecuteAsync(reader, provider);
            }

            await reader.CursorAsync(z => z.Version < MaxVersion, async z => await this.ExecuteAsync(provider, z));
        }

        private async Task ExecuteAsync(IObjectEditProvider<T> provider, T item)
        {
            Debug.Assert(item.Version < MaxVersion);

            if (item.Version < MaxVersion)
            {
                var upgrade = false;
                for (var i = item.Version + 1; i < Patchs.Length; i++)
                {
                    var patch = Patchs[i];
                    var gpatch = patch as IGlobalPatch<T>;
                    if (gpatch != null ? await gpatch.UpgradeAsync(this.dataCenter, item) : await patch.UpgradeAsync(item))
                    {
                        upgrade = true;
                        item.Version = i;
                    }
                    else
                    {
                        break;
                    }
                }
                if (upgrade) await provider.UpdateAsync(item);
            }
        }
    }
}