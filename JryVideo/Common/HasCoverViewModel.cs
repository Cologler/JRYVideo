using Jasily.ComponentModel;
using JryVideo.Core;
using JryVideo.Model;
using System.Diagnostics;
using System.Threading.Tasks;

namespace JryVideo.Common
{
    public abstract class HasCoverViewModel<T> : JasilyViewModel<T>
        where T : IJryCoverParent
    {
        private JryCover cover;

        protected HasCoverViewModel(T source)
            : base(source)
        {
        }

        [NotifyPropertyChanged]
        public JryCover Cover
        {
            get
            {
                this.BeginUpdateCoverIfEmpty();
                return this.cover;
            }
            set { this.SetPropertyRef(ref this.cover, value); }
        }

        public async Task<JryCover> TryGetCoverAsync()
        {
            if (this.Source.CoverId == null) return null;

            return await JryVideoCore.Current.CurrentDataCenter.CoverManager.LoadCoverAsync(this.Source.CoverId);
        }

        protected virtual Task<bool> TryAutoAddCoverAsync() => Task.FromResult(false);

        public void BeginUpdateCoverIfEmpty()
        {
            if (this.cover != null) return;
            this.BeginUpdateCover();
        }

        public async void BeginUpdateCover()
        {
            if (this.Source.CoverId == null && !await this.TryAutoAddCoverAsync()) return;

            Debug.Assert(this.Source.CoverId != null);
            if (this.IsDelayLoad) await Task.Yield();
            this.Cover = await this.GetManagers().CoverManager.LoadCoverAsync(this.Source.CoverId);
            if (this.cover == null)
            {
                var errorMsg = $"{this.Source} missing cover [{this.Source.CoverId}].";
                await Log.WriteAsync(errorMsg);
                if (Debugger.IsAttached) Debugger.Break();
            }
        }

        protected virtual bool IsDelayLoad => false;
    }
}