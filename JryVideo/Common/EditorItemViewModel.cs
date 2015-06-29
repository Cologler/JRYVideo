using System;
using System.ComponentModel;
using System.Enums;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Common
{
    public abstract class EditorItemViewModel<T> : JasilyViewModel<T>
        where T : JryObject, new()
    {
        public event EventHandler<string[]> FindErrorMessages;
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
        }

        public T GetCommitObject()
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
            var error = obj.FireObjectError().ToArray();
            
            if (error.Length > 0)
            {
                this.FindErrorMessages.Fire(this, error);
                return null;
            }

            switch (this.Action)
            {
                case ObjectChangedAction.Create:
                    if (await provider.InsertAsync(obj))
                    {
                        this.Created.BeginFire(this, obj);
                        return obj;
                    }
                    else
                    {
                        return null;
                    }
                    break;

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
                    break;

                case ObjectChangedAction.Replace:
                case ObjectChangedAction.Delete:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}