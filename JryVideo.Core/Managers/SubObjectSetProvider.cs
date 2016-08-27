using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Model;

namespace JryVideo.Core.Managers
{
    internal class SubObjectSetProvider<T, TSub> : IJasilyEntitySetProvider<TSub, string>
        where T : JryObject, IJryChild<TSub>
        where TSub : JryObject, IJasilyEntity<string>
    {
        private readonly IObjectEditProvider<T> parentManager;

        public T Parent { get; }

        public SubObjectSetProvider(IObjectEditProvider<T> parentManager, T parent)
        {
            this.parentManager = parentManager;
            this.Parent = parent;
        }

        protected List<TSub> ObjectSet => this.Parent.Childs;

        public Task<IDictionary<string, TSub>> FindAsync(IEnumerable<string> ids)
        {
            var array = ids.ToArray();
            return Task.FromResult((IDictionary<string, TSub>)this.ObjectSet.Where(z => array.Contains(z.Id)).ToDictionary(z => z.Id));
        }

        public Task<TSub> FindAsync(string id)
            => Task.FromResult(this.ObjectSet.FirstOrDefault(z => z.Id == id));

        public async Task<bool> InsertAsync(TSub entity)
        {
            this.ObjectSet.Add(entity);
            return await this.parentManager.UpdateAsync(this.Parent);
        }

        public async Task<bool> InsertAsync(IEnumerable<TSub> items)
        {
            this.ObjectSet.AddRange(items);
            return await this.parentManager.UpdateAsync(this.Parent);
        }

        public async Task<bool> InsertOrUpdateAsync(TSub entity)
        {
            this.ObjectSet.RemoveAll(z => z.Id == entity.Id);
            return await this.InsertAsync(entity);
        }

        public Task<IEnumerable<TSub>> ListAsync(int skip, int take)
            => Task.FromResult((IEnumerable<TSub>)this.ObjectSet.Skip(skip).Take(take).ToArray());

        public Task CursorAsync(Action<TSub> callback)
        {
            foreach (var sub in this.ObjectSet)
            {
                callback(sub);
            }
            return Task.FromResult(0);
        }

        public Task CursorAsync(Expression<Func<TSub, bool>> filter, Action<TSub> callback)
        {
            var con = filter.Compile();
            foreach (var sub in this.ObjectSet.Where(con))
            {
                callback(sub);
            }
            return Task.FromResult(0);
        }

        public async Task<bool> RemoveAsync(string id)
        {
            return this.ObjectSet.RemoveAll(z => z.Id == id) > 0 &&
                   await this.parentManager.UpdateAsync(this.Parent);
        }

        public async Task<bool> UpdateAsync(TSub entity)
        {
            var index = this.ObjectSet.FindIndex(z => z.Id == entity.Id);
            if (index == -1) return false;
            this.ObjectSet[index] = entity;
            return await this.parentManager.UpdateAsync(this.Parent);
        }
    }
}