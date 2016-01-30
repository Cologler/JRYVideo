using JryVideo.Model;
using System.Data;
using System.Threading.Tasks;

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

        protected override T BuildItem(string id) => VideoInfoAttached.Build<T>(id);
    }
}