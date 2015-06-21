using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using JryVideo.Core;
using JryVideo.Core.Douban;
using JryVideo.Model;

namespace JryVideo.EditCover
{
    public class EditCoverViewModel : JasilyViewModel<JryCover>
    {
        private string _doubanId;
        private string _uri;
        private byte[] _binaryData;
        private BitmapImage _bitmapImage;
        private string _metaData;

        public EditCoverViewModel(JryCover source)
            : base(source)
        {
            this.DoubanId = this.Source.DoubanId;
            this.Uri = this.Source.Uri;
            this.BinaryData = this.Source.BinaryData;
        }

        public string DoubanId
        {
            get { return this._doubanId; }
            set { this.SetPropertyRef(ref this._doubanId, value); }
        }

        public string Uri
        {
            get { return this._uri; }
            set { this.SetPropertyRef(ref this._uri, value); }
        }

        public byte[] BinaryData
        {
            get { return this._binaryData; }
            set
            {
                if (this.SetPropertyRef(ref this._binaryData, value))
                    this.OnUpdatedBinaryData();
            }
        }

        public BitmapImage BitmapImage
        {
            get { return this._bitmapImage; }
            set { this.SetPropertyRef(ref this._bitmapImage, value); }
        }

        public JryCoverSourceType CoverSourceType { get; set; }

        public string MetaData
        {
            get { return this._metaData; }
            private set { this.SetPropertyRef(ref this._metaData, value); }
        }

        private void OnUpdatedBinaryData()
        {
            this.BitmapImage = this.BinaryData == null ? null : this.LoadImage(this.BinaryData);

            this.MetaData = this.BitmapImage == null
                ? "W x H: 0 x 0"
                : String.Format("W x H: {0} x {1}", this.BitmapImage.PixelWidth, this.BitmapImage.PixelHeight);
        }

        private BitmapImage LoadImage(byte[] data)
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = data.ToMemoryStream();
                image.EndInit();
                return image;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> LoadFromUrlAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Uri)) return false;

            var request = WebRequest.CreateHttp(this.Uri);

            var result = await request.GetResultAsBytesAsync();

            if (result.IsSuccess)
            {
                this.BinaryData = result.Result;
            }

            return this.BinaryData != null;
        }

        public async Task<bool> LoadFromDoubanAsync()
        {
            if (string.IsNullOrWhiteSpace(this.DoubanId)) return false;

            var json = await DoubanHelper.GetMovieInfoAsync(this.DoubanId);

            if (json == null || json.Images == null || json.Images.Large == null)
            {
                return false;
            }

            this.Uri = DoubanHelper.GetLargeImageUrl(json);

            var request = WebRequest.CreateHttp(this.Uri);

            this.BinaryData = (await request.GetResultAsBytesAsync()).Result;

            return this.BinaryData != null;
        }

        public async Task AcceptAsync()
        {
            var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;

            this.Source.CoverSourceType = this.CoverSourceType;
            
            this.Source.DoubanId = this.DoubanId;
            this.Source.Uri = this.Uri;

            this.Source.BinaryData = this.BinaryData;

            await coverManager.UpdateAsync(this.Source);
        }
    }
}