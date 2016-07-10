using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Jasily.ComponentModel;
using JryVideo.Model;

namespace JryVideo.Common
{
    public class CoverViewModel : JasilyViewModel<Model.Interfaces.ICoverParent>
    {
        private JryCover cover;
        private BitmapImage bitmapImage;

        public CoverViewModel(Model.Interfaces.ICoverParent source)
            : base(source)
        {
        }

        /// <summary>
        /// only work on UI thread.
        /// </summary>
        [NotifyPropertyChanged]
        public JryCover Cover
        {
            get
            {
                try
                {
                    if (this.cover == null)
                    {
                        this.BeginForceReloadCover();
                    }
                    else
                    {

                    }
                    return this.cover;
                }
                finally
                {
                    this.cover = null;
                }
            }
            private set
            {
                this.SetPropertyRef(ref this.cover, value);
            }
        }

        public BitmapImage BitmapImage
        {
            get
            {
                if (this.bitmapImage == null)
                {
                    this.BeginLoadCover(z =>
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = z.BinaryStream;
                        image.CacheOption = BitmapCacheOption.OnDemand;
                        //image.CreateOptions = BitmapCreateOptions.DelayCreation;
                        image.DecodePixelWidth = 300;
                        image.EndInit();
                        this.BitmapImage = image;
                    });
                }
                return this.bitmapImage;
            }
            set { this.SetPropertyRef(ref this.bitmapImage, value); }
        }

        /// <summary>
        /// return null if get failed.
        /// </summary>
        /// <returns></returns>
        public async Task<JryCover> GetCoverAsync() => await this.GetManagers().CoverManager.LoadCoverAsync(this.Source.CoverId);

        public void BeginForceReloadCover() => this.BeginLoadCover(z => this.Cover = z);

        public async void BeginLoadCover(Action<JryCover> callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
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
            callback(cover);
        }

        public bool IsDelayLoad { get; set; }

        public IAutoGenerateCoverProvider<Model.Interfaces.ICoverParent> AutoGenerateCoverProvider { get; set; }
    }
}