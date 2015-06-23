using System;
using System.ComponentModel;
using System.Enums;
using System.Net;
using System.Threading.Tasks;
using JryVideo.Common;
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
        private ImageViewModel _imageViewModel;

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

        public JryCoverSourceType CoverSourceType { get; set; }

        public ImageViewModel ImageViewModel
        {
            get { return this._imageViewModel; }
            private set { this.SetPropertyRef(ref this._imageViewModel, value); }
        }

        public ObjectChangedAction Action { get; set; }

        private void OnUpdatedBinaryData()
        {
            this.ImageViewModel = this.BinaryData == null ? null : ImageViewModel.Build(this.BinaryData);
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

            var json = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);

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

            switch (this.Action)
            {
                case ObjectChangedAction.Create:
                    await coverManager.InsertAsync(this.Source);
                    break;

                case ObjectChangedAction.Modify:
                    await coverManager.UpdateAsync(this.Source);
                    break;

                case ObjectChangedAction.Replace:
                case ObjectChangedAction.Delete:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}