using JryVideo.Model;
using System;
using System.Collections.Generic;
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

        public async Task RunAsync()
        {
            await Task.Run(async () =>
            {
                await this.dataCenter.CoverManager.Source.CursorAsync(z => this.covers.Add(z.Id, false));
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
                this.missingSource.AddRange(this.covers.Where(z => !z.Value).Select(z => z.Key));
                
                Log.Write($"cover missing source ({this.missingSource.Count}):".IntoArray().Concat(this.missingSource).AsLines());
                Log.Write($"cover missing entity ({this.missingCover.Count}):".IntoArray().Concat(this.missingCover).AsLines());
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
                this.missingCover.Add(coverId);
            }
        }

        private readonly Dictionary<string, bool> covers = new Dictionary<string, bool>();
        private readonly List<string> missingCover = new List<string>();
        private readonly List<string> missingSource = new List<string>();

        public async Task FixAsync()
        {
            // remove all missing source cover
            foreach (var id in this.missingSource)
            {
                await this.dataCenter.CoverManager.RemoveAsync(id);
            }
        }
    }
}