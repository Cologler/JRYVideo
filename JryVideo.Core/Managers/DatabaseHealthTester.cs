using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public class DatabaseHealthTester
    {
        private readonly DataCenter dataCenter;

        public DatabaseHealthTester(DataCenter dataCenter)
        {
            this.dataCenter = dataCenter;
        }

        [Conditional("DEBUG")]
        public void RunOnDebugAsync()
        {
            // await this.RunAsync(false);
        }

        public async Task RunAsync(bool fix)
        {
            await Task.Run(async () =>
            {
                await this.dataCenter.CoverManager.Source.CursorAsync(z =>
                {
                    this.covers.Add(z.Id, false);
                });
                await this.dataCenter.VideoRoleManager.Source.CursorAsync(z =>
                {
                    z.MajorRoles?.ForEach(this.ConnectToCover);
                    z.MinorRoles?.ForEach(this.ConnectToCover);
                });
                await this.dataCenter.SeriesManager.Source.CursorAsync(z =>
                {
                    z.Videos.ForEach(this.ConnectToCover);
                    z.Videos.Select(x => x.BackgroundImageId).ForEach(this.ConnectToCover);
                });
                this.coverMissingSource.AddRange(this.covers.Where(z => !z.Value).Select(z => z.Key));

                var log =
                    $"cover missing source ({this.coverMissingSource.Count}):".IntoArray()
                        .Concat(this.coverMissingSource).Concat(
                    $"cover missing entity ({this.coverMissingEntity.Count}):".IntoArray()
                        .Concat(this.coverMissingEntity)).AsLines();

                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(log);
                }
                else
                {
                    Log.Write(log);
                }

                // remove all missing source cover
                if (fix)
                {
                    foreach (var id in this.coverMissingSource)
                    {
                        await this.dataCenter.CoverManager.RemoveAsync(id);
                    }
                }
            });
        }

        private void ConnectToCover(IJryCoverParent obj) => this.ConnectToCover(obj.CoverId);

        private void ConnectToCover(string coverId)
        {
            if (coverId == null) return;
            if (this.covers.ContainsKey(coverId))
            {
                this.covers[coverId] = true;
            }
            else
            {
                this.coverMissingEntity.Add(coverId);
            }
        }

        private readonly Dictionary<string, bool> covers = new Dictionary<string, bool>();
        private readonly List<string> coverMissingEntity = new List<string>();
        private readonly List<string> coverMissingSource = new List<string>();
    }
}