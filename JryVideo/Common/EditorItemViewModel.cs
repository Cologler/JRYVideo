using System;
using System.ComponentModel;
using System.ComponentModel.Editable;
using System.Diagnostics;
using System.Enums;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Common
{
    public abstract class EditorItemViewModel<T> : JasilyEditableViewModel<T>
        where T : JryObject, new()
    {
        public event EventHandler<T> Created;
        public event EventHandler<T> Updated;

        public EditorItemViewModel()
            : base(null)
        {
        }

        public virtual void CreateMode()
        {
            this.Source = new T();
            this.Action = ObjectChangedAction.Create;
        }

        public virtual void ModifyMode(T source)
        {
            this.Source = source.ThrowIfNull("source");
            this.Action = ObjectChangedAction.Modify;

            this.ReadFromObject(source);
        }

        public virtual T GetCommitObject()
        {
            return this.Action == ObjectChangedAction.Create ? new T() : this.Source;
        }

        public ObjectChangedAction Action { get; private set; }

        /// <summary>
        /// please sure call obj.BuildMetaData() before call this.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected async Task<T> CommitAsync(IObjectEditProvider<T> provider, T obj)
        {
            if (obj.HasError())
            {
                if (Debugger.IsAttached) Debugger.Break();
                return null;
            }

            switch (this.Action)
            {
                case ObjectChangedAction.Create:
                    if (await provider.InsertAsync(obj))
                    {
                        this.Clear();
                        this.Created.BeginFire(this, obj);
                        return obj;
                    }
                    else
                    {
                        return null;
                    }

                case ObjectChangedAction.Modify:
                    if (await provider.UpdateAsync(obj))
                    {
                        this.Updated.BeginFire(this, obj);
                        return obj;
                    }
                    else
                    {
                        return null;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void Clear()
        {
            
        }
    }
}