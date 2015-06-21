using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace JryVideo.Common
{
    public class ImageViewModel : JasilyViewModel<BitmapImage>
    {
        private string _pixelMetaData;

        private ImageViewModel(BitmapImage source)
            : base(source)
        {
            this._pixelMetaData = String.Format("W x H: {0} x {1}", source.PixelWidth, source.PixelHeight);
        }

        public string PixelMetaData
        {
            get { return this._pixelMetaData; }
            set { this.SetPropertyRef(ref this._pixelMetaData, value); }
        }

        public static ImageViewModel Build(byte[] data)
        {
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = data.ToMemoryStream();
                image.EndInit();
                return new ImageViewModel(image);
            }
            catch
            {
                return null;
            }
        }
    }
}