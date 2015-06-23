using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Data.DataSources;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    public class JryObjectManager<T, TProvider>
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

        public async virtual Task<T> QueryAsync(string id)
        {
            return await this.Source.QueryAsync(id);
        }

        public async virtual Task<bool> InsertAsync(T obj)
        {
            if (obj.CheckError().ToArray().Length > 0) return false;

            return await this.Source.InsertAsync(obj);
        }

        public async virtual Task<bool> UpdateAsync(T obj)
        {
            if (obj.CheckError().ToArray().Length > 0) return false;

            return await this.Source.UpdateAsync(obj);
        }
    }
}