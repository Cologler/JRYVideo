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
            this.UpgraderInfos.Add(new UpgraderInfoV0());
            this.UpgraderInfos.Add(new UpgraderInfoV1(dataCenter));
#if DEBUG
            for (var i = 0; i < this.UpgraderInfos.Count; i++)
            {
                Debug.Assert(this.UpgraderInfos[i].Version == i);
            }
#endif
            Debug.Assert(this.UpgraderInfos.Count > 0);
            this.Version = this.UpgraderInfos.Count - 1;
        }

        public int Version { get; }

        public DataCenter DataCenter { get; }

        public List<IUpgraderInfo> UpgraderInfos { get; } = new List<IUpgraderInfo>();

        private async Task CallbackAsync<T>(IObjectEditProvider<T> provider, T item, Func<IUpgraderInfo, T, Task> callback)
            where T : IObject
        {
            if (item.Version < this.Version)
            {
                for (var i = item.Version + 1; i < this.UpgraderInfos.Count; i++)
                {
                    await callback(this.UpgraderInfos[i], item);
                }
                item.Version = this.Version;
                await provider.UpdateAsync(item);
            }
        }

        public async Task RunAsync()
        {
            await this.DataCenter.SeriesManager.Source.CursorAsync(async z =>
            {
                await this.CallbackAsync(this.DataCenter.SeriesManager, z, async (info, series) =>
                {
                    await info.UpgradeAsync(series);
                });
            });

            await this.DataCenter.CoverManager.Source.CursorAsync(async z =>
            {
                await this.CallbackAsync(this.DataCenter.CoverManager, z, async (info, cover) =>
                {
                    await info.UpgradeAsync(cover);
                });
            });

            Debug.WriteLine("upgrade completed.");
        }
    }
}