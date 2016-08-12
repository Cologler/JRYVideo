using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Jasily.ComponentModel.Editable;
using Jasily.Diagnostics;
using JryVideo.Core.Managers;
using JryVideo.Model;

namespace JryVideo.Common
{
    public abstract class EditorItemViewModel<T> : JasilyEditableViewModel<T>
        where T : JryObject, new()
    {
        private string description;
        public event EventHandler<RequestActionEventArgs<T>> Creating;
        public event EventHandler<T> Created;
        public event EventHandler<T> Updated;
        private T editingObject;

        public virtual void ModifyMode(T source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            this.Action = ObjectChangedAction.Modify;
            this.editingObject = source;
            this.ReadFromObject(this.editingObject);
        }

        public virtual void CreateMode()
        {
            this.Action = ObjectChangedAction.Create;
            this.editingObject = this.NewInstance();
        }

        public virtual void CloneMode(T source)
        {
            this.Action = ObjectChangedAction.Create;
            this.editingObject = this.NewInstance();
            this.ReadFromObject(source);
        }

        protected virtual T NewInstance()
        {
            var obj = new T();
            obj.BuildMetaData();
            return obj;
        }

        [EditableField]
        public string Description
        {
            get { return this.description; }
            set { this.SetPropertyRef(ref this.description, value); }
        }

        public T GetCommitObject() => this.editingObject;

        public ObjectChangedAction? Action { get; private set; }

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
                    obj.ResetCreated();
                    var arg = new RequestActionEventArgs<T>(obj) { IsAccept = true };
                    this.Creating.Fire(this, arg);
                    if (arg.IsAccept && await provider.InsertAsync(obj))
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
                    if (await this.OnUpdateAsync(provider, obj))
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

        protected virtual async Task<bool> OnUpdateAsync(IObjectEditProvider<T> provider, T obj)
            => await provider.UpdateAsync(obj);

        protected virtual void Clear()
        {

        }

        #region Overrides of JasilyEditableViewModel<T>

        public override void ReadFromObject(T obj)
        {
            JasilyDebug.Pointer();
            base.ReadFromObject(obj);
            JasilyDebug.Pointer();
        }

        public override void WriteToObject(T obj)
        {
            JasilyDebug.Pointer();
            base.WriteToObject(obj);
            JasilyDebug.Pointer();
        }

        #endregion

        public class NameEditableViewModel<TNameable> : JasilyEditableViewModel<TNameable>
            where TNameable : INameable
        {
            private readonly bool nullIfEmpty;
            private string names = string.Empty;

            public NameEditableViewModel(bool nullIfEmpty)
            {
                this.nullIfEmpty = nullIfEmpty;
            }

            [EditableField(Converter = typeof(MulitLineConverter))]
            public string Names
            {
                get { return this.names; }
                set { this.SetPropertyRef(ref this.names, value); }
            }

            public override void WriteToObject(TNameable obj)
            {
                base.WriteToObject(obj);
                if (obj.Names == null && !this.nullIfEmpty) obj.Names = new List<string>();
            }

            public void AddRange(IEnumerable<string> items)
                => this.Names = this.Names.AsLines(StringSplitOptions.RemoveEmptyEntries).Concat(items).AsLines();
        }
    }
}