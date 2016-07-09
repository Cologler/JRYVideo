using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Managers.Upgrades
{
    public class Upgrader
    {
        private readonly List<IPatch> patchs = new List<IPatch>();
        private readonly Upgrader<JrySeries> seriesUpgrader;
        private readonly Upgrader<JryCover> coverUpgrader;

        public Upgrader(DataCenter dataCenter)
        {
            this.DataCenter = dataCenter;
            this.patchs.Add(new Patch0000());
            this.patchs.Add(new Patch0001(dataCenter));
            this.patchs.Add(new Patch0002());

            this.seriesUpgrader = new Upgrader<JrySeries>(this.patchs);
            this.coverUpgrader = new Upgrader<JryCover>(this.patchs);
        }

        public DataCenter DataCenter { get; }

        public async Task RunAsync()
        {
            await this.seriesUpgrader.ExecuteAsync(this.DataCenter.SeriesManager.Source, this.DataCenter.SeriesManager);
            await this.coverUpgrader.ExecuteAsync(this.DataCenter.CoverManager.Source, this.DataCenter.CoverManager);

            Debug.WriteLine("upgrade completed.");
        }
    }

    public class Upgrader<T>
        where T : class, IObject
    {
        private readonly IPatch<T>[] patchs;
        private readonly IEveryTimePatch<T>[] everyTimePatchs;
        private readonly int maxVersion;

        public Upgrader(IEnumerable<IPatch> patchs)
        {
            var ps = patchs.ToArray();
            this.patchs = ps.OfType<IPatch<T>>().ToArray();
            this.maxVersion = this.patchs.Length - 1;
            this.everyTimePatchs = ps.OfType<IEveryTimePatch<T>>().ToArray();
        }

        public async Task ExecuteAsync(IJasilyEntitySetReader<T, string> reader, IObjectEditProvider<T> provider)
        {
            foreach (var patch in this.everyTimePatchs)
            {
                await patch.ExecuteAsync(reader, provider);
            }

            await reader.CursorAsync(z => z.Version < this.maxVersion, async z => await this.ExecuteAsync(provider, z));
        }

        private async Task ExecuteAsync(IObjectEditProvider<T> provider, T item)
        {
            Debug.Assert(item.Version < this.maxVersion);

            if (item.Version < this.maxVersion)
            {
                var upgrade = false;
                for (var i = item.Version + 1; i < this.patchs.Length; i++)
                {
                    var patch = this.patchs[i];
                    if (await patch.UpgradeAsync(item))
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