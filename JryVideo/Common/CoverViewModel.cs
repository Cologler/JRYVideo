using System.Diagnostics;
using System.Threading.Tasks;
using Jasily.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class CoverViewModel : JasilyViewModel<Model.Interfaces.ICoverParent>
    {
        private JryCover cover;

        public CoverViewModel(Model.Interfaces.ICoverParent source)
            : base(source)
        {
        }

        [NotifyPropertyChanged]
        public JryCover Cover
        {
            get
            {
                if (this.cover == null)
                {
                    this.BeginForceReloadCover();
                }
                return this.cover;
            }
            private set { this.SetPropertyRef(ref this.cover, value); }
        }

        public async Task<JryCover> TryGetCoverAsync()
        {
            if (this.Source.CoverId == null) return null;

            return await this.GetManagers().CoverManager.LoadCoverAsync(this.Source.CoverId);
        }

        public async void BeginForceReloadCover()
        {
            var coverId = this.Source.CoverId;
            JryCover cover = null;
            if (coverId != null)
            {
                cover = await this.GetManagers().CoverManager.LoadCoverAsync(coverId);
            }
            if (cover == null)
            {
                var generater = this.AutoGenerateCoverProvider;
                if (generater == null || !await generater.GenerateAsync(this.Source))
                {
                    return;
                }
                Debug.Assert(this.Source.CoverId != null);
                cover = await this.GetManagers().CoverManager.LoadCoverAsync(coverId);
                Debug.Assert(cover != null);
            }
            if (this.IsDelayLoad) await Task.Yield();
            this.Cover = cover;
        }

        public bool IsDelayLoad { get; set; }

        public IAutoGenerateCoverProvider<Model.Interfaces.ICoverParent> AutoGenerateCoverProvider { get; set; }
    }
}