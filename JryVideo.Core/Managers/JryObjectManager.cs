using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class JryObjectManager<T, TProvider> : IObjectEditProvider<T>
        where T : JryObject
        where TProvider : IDataSourceProvider<T>
    {
        protected JryObjectManager(TProvider source)
        {
            this.Source = source;
        }

        protected TProvider Source { get; private set; }

        public async virtual Task<IEnumerable<T>> LoadAsync()
        {
            return await this.Source.QueryAsync(0, Int32.MaxValue);
        }

        public async virtual Task<IEnumerable<T>> LoadAsync(int skip, int take)
        {
            return await this.Source.QueryAsync(skip, take);
        }

        public async virtual Task<T> FindAsync(string id)
        {
            return await this.Source.FindAsync(id);
        }

        public async virtual Task<bool> InsertAsync(T obj)
        {
            if (obj.HasError()) return false;

            return await this.Source.InsertAsync(obj);
        }

        public async virtual Task<bool> InsertAsync(IEnumerable<T> objs)
        {
            if (objs.Any(obj => obj.HasError())) return false;

            return await this.Source.InsertAsync(objs);
        }

        public async virtual Task<bool> UpdateAsync(T obj)
        {
            if (obj.HasError()) return false;

            return await this.Source.UpdateAsync(obj);
        }

        public async virtual Task<bool> RemoveAsync(string id)
        {
            return await this.Source.RemoveAsync(id);
        }
    }
}