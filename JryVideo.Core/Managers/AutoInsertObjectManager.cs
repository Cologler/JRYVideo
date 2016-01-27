using JryVideo.Model;
using System.Data;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public abstract class AutoInsertObjectManager<T, TProvider> : JryObjectManager<T, TProvider>
        where T : JryObject
        where TProvider : IJasilyEntitySetProvider<T, string>
    {
        public AutoInsertObjectManager(TProvider source)
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

        protected abstract T BuildItem(string id);
    }
}