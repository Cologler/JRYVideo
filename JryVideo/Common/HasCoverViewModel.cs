using Jasily.ComponentModel;
using JryVideo.Core;
using JryVideo.Model;
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
                var cover = this.cover;

                if (cover == null)
                {
                    this.BeginUpdateCover();
                    return null;
                }
                else
                {
                    return cover;
                }
            }
            set
            {
                this.SetPropertyRef(ref this.cover, value);
            }
        }

        public async Task<JryCover> TryGetCoverAsync()
        {
            if (this.Source.CoverId == null) return null;

            return await JryVideoCore.Current.CurrentDataCenter.CoverManager.LoadCoverAsync(this.Source.CoverId);
        }

        protected abstract Task<bool> TryAutoAddCoverAsync();

        public virtual async void BeginUpdateCover()
        {
            if (this.Source.CoverId != null || await this.TryAutoAddCoverAsync())
            {
                var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;
                this.Cover = await coverManager.LoadCoverAsync(this.Source.CoverId);
            }
        }
    }
}