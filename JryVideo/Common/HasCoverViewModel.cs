using System.ComponentModel;
using System.Threading.Tasks;
using JryVideo.Core;
using JryVideo.Model;

namespace JryVideo.Common
{
    public abstract class HasCoverViewModel<T> : JasilyViewModel<T>
        where T : IJryCoverParent
    {
        private JryCover _cover;

        protected HasCoverViewModel(T source)
            : base(source)
        {
        }

        public JryCover Cover
        {
            get
            {
                var cover = this._cover;

                if (cover == null)
                {
                    this.BeginUpdateCover();
                    return null;
                }
                else
                {
                    this._cover = null; // clear memery.
                    return cover;
                }
            }
            set
            {
                this.SetPropertyRef(ref this._cover, value);
            }
        }

        public async Task<JryCover> TryGetCoverAsync()
        {
            if (this.Source.CoverId == null) return null;

            return await JryVideoCore.Current.CurrentDataCenter.CoverManager.LoadCoverAsync(this.Source.CoverId);
        }

        protected virtual async Task<bool> TryAutoAddCoverAsync()
        {
            return false;
        }

        public async void BeginUpdateCover()
        {
            if (this.Source.CoverId != null || await this.TryAutoAddCoverAsync())
            {
                var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;
                this.Cover = await coverManager.LoadCoverAsync(this.Source.CoverId);
            }
        }
    }
}