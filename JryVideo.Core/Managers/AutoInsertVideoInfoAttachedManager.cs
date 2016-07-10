using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Core.Managers.Upgrades;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class AutoInsertVideoInfoAttachedManager<T, TProvider> : AutoInsertObjectManager<T, TProvider>
        where T : VideoInfoAttached, new()
        where TProvider : IJasilyEntitySetProvider<T, string>
    {
        protected AutoInsertVideoInfoAttachedManager(TProvider source)
            : base(source)
        {
        }

        public override async Task<T> FindAsync(string id)
        {
            var item = await base.FindAsync(id);
            if (item != null) return item;
            await this.InsertOrUpdateAsync(this.BuildItem(id));
            return await base.FindAsync(id);
        }

        protected override T BuildItem(string id)
        {
            var ret = VideoInfoAttached.Build<T>(id);
            ret.Version = Upgrader<T>.MaxVersion; // for jryvideo & video role collection
            return ret;
        }
    }
}