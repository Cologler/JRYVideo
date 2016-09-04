using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.EventArgses;
using System.Threading.Tasks;
using Jasily.Data;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Core.Managers
{
    public class JryObjectManager<T, TProvider> : IObjectEditProvider<T>
        where T : JryObject
        where TProvider : IJasilyEntitySetProvider<T, string>
    {
        internal event EventHandler<T> ItemCreated;
        internal event EventHandler<T> ItemCreatedOrUpdated;
        internal event EventHandler<ChangingEventArgs<T>> ItemUpdated;
        internal event EventHandler<T> ItemRemoved;
        internal event EventHandler<ICoverParent> CoverParentRemoving;

        protected JryObjectManager(TProvider source)
        {
            this.Source = source;
        }

        internal TProvider Source { get; }

        public async Task<IEnumerable<T>> LoadAsync()
            => await this.Source.ListAsync(0, int.MaxValue);

        public async Task<IEnumerable<T>> LoadAsync(int skip, int take)
            => await this.Source.ListAsync(skip, take);

        public virtual async Task<T> FindAsync(string id)
            => await this.Source.FindAsync(id);

        public virtual async Task<bool> InsertAsync(T obj)
        {
            if (await this.InsertCoreAsync(obj))
            {
                this.ItemCreated?.BeginInvoke(this, obj);
                return true;
            }
            return false;
        }

        protected virtual async Task<bool> InsertCoreAsync(T obj)
        {
            obj.CheckError();
            return await this.Source.InsertAsync(obj);
        }

        public async Task<bool> InsertOrUpdateAsync(T obj)
        {
            obj.CheckError();
            if (await this.Source.InsertOrUpdateAsync(obj))
            {
                this.ItemCreatedOrUpdated?.BeginInvoke(this, obj);
                return true;
            }
            return false;
        }

        public virtual async Task<bool> UpdateAsync(T obj)
        {
            obj.CheckError();
            var old = await this.Source.FindAsync(obj.Id);
            Debug.Assert(obj != null);
            if (await this.Source.UpdateAsync(obj))
            {
                this.ItemUpdated?.BeginInvoke(this, new ChangingEventArgs<T>(old, obj));
                return true;
            }
            return false;
        }

        public virtual async Task<bool> RemoveAsync(T obj)
        {
            if (await this.Source.RemoveAsync(obj.Id))
            {
                this.ItemRemoved?.BeginInvoke(this, obj);
                return true;
            }
            return false;
        }

        public virtual async Task<bool> RemoveAsync(string id)
        {
            if (await this.Source.RemoveAsync(id))
            {
                return true;
            }
            return false;
        }

        protected void OnCoverParentRemoving(ICoverParent parent, object sender = null)
            => this.CoverParentRemoving?.Invoke(sender ?? this, parent);

        internal async Task<CombineResult> CanCombineAsync(string to, string from)
        {
            var obj1 = await this.FindAsync(to);
            if (obj1 == null) return CombineResult.NotFound;
            var obj2 = await this.FindAsync(from);
            if (obj2 == null) return CombineResult.NotFound;
            return await this.CanCombineAsync(obj1, obj2);
        }

        internal async Task<CombineResult> CombineAsync(string to, string from)
        {
            var obj1 = await this.FindAsync(to);
            if (obj1 == null) return CombineResult.NotFound;
            var obj2 = await this.FindAsync(from);
            if (obj2 == null) return CombineResult.NotFound;
            return await this.CombineAsync(obj1, obj2);
        }

        internal virtual Task<CombineResult> CanCombineAsync(T to, T from)
            => Task.FromResult(CombineResult.NotSupported);

        internal virtual Task<CombineResult> CombineAsync(T to, T from)
            => Task.FromResult(CombineResult.NotSupported);
    }
}