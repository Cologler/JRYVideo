using JryVideo.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JryVideo.Core.Managers
{
    public class JryObjectManager<T, TProvider> : IObjectEditProvider<T>
        where T : JryObject
        where TProvider : IJasilyEntitySetProvider<T, string>
    {
        protected JryObjectManager(TProvider source)
        {
            this.Source = source;
        }

        public TProvider Source { get; }

        public async Task<IEnumerable<T>> LoadAsync()
        {
            return await this.Source.ListAsync(0, Int32.MaxValue);
        }

        public async Task<IEnumerable<T>> LoadAsync(int skip, int take)
        {
            return await this.Source.ListAsync(skip, take);
        }

        public virtual async Task<T> FindAsync(string id)
        {
            return await this.Source.FindAsync(id);
        }

        public virtual async Task<bool> InsertAsync(T obj)
        {
            obj.Saving();
            if (obj.HasError()) return false;

            return await this.Source.InsertAsync(obj);
        }

        public async Task<bool> InsertOrUpdateAsync(T obj)
        {
            obj.Saving();
            if (obj.HasError()) return false;

            return await this.Source.InsertOrUpdateAsync(obj);
        }

        protected virtual async Task<bool> InsertAsync(IEnumerable<T> objs)
        {
            var items = objs as T[] ?? objs.ToArray();
            items.ForEach(z => z.Saving());
            if (items.Any(obj => obj.HasError())) return false;

            return await this.Source.InsertAsync(items);
        }

        public virtual async Task<bool> UpdateAsync(T obj)
        {
            obj.Saving();
            if (obj.HasError()) return false;

            return await this.Source.UpdateAsync(obj);
        }

        public virtual async Task<bool> RemoveAsync(string id)
        {
            return await this.Source.RemoveAsync(id);
        }
    }
}