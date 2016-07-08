using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Managers.Upgrades
{
    public class Upgrader
    {
        public Upgrader(DataCenter dataCenter)
        {
            this.DataCenter = dataCenter;
            this.Patchs.Add(new UpgraderInfoV0());
            this.Patchs.Add(new UpgraderInfoV1(dataCenter));
        }

        public int MaxVersion => this.Patchs.Count - 1;

        public DataCenter DataCenter { get; }

        public List<IUpgraderInfo> Patchs { get; } = new List<IUpgraderInfo>();

        private async Task CallbackAsync<T>(IObjectEditProvider<T> provider, T item, Func<IUpgraderInfo, T, Task<bool>> callback)
            where T : IObject
        {
            if (item.Version < this.MaxVersion)
            {
                var upgrade = false;
                for (var i = item.Version + 1; i < this.Patchs.Count; i++)
                {
                    var info = this.Patchs[i];
                    if (await callback(info, item))
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

        public async Task RunAsync()
        {
            await this.DataCenter.SeriesManager.Source.CursorAsync(z => z.Version < this.MaxVersion, async z =>
            {
                await this.CallbackAsync(this.DataCenter.SeriesManager, z, async (info, series) => await info.UpgradeAsync(series));
            });

            await this.DataCenter.CoverManager.Source.CursorAsync(z => z.Version < this.MaxVersion, async z =>
            {
                await this.CallbackAsync(this.DataCenter.CoverManager, z, async (info, cover) => await info.UpgradeAsync(cover));
            });

            Debug.WriteLine("upgrade completed.");
        }
    }
}