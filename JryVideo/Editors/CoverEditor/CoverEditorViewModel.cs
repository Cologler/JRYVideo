using System.Enums;
using System.Net;
using System.Threading.Tasks;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.Douban;
using JryVideo.Model;

namespace JryVideo.Editors.CoverEditor
{
    public class CoverEditorViewModel : EditorItemViewModel<JryCover>
    {
        private string doubanId;
        private string uri;
        private byte[] binaryData;
        private ImageViewModel imageViewModel;

        public override void CreateMode()
        {
            this.DoubanId = this.Uri = null;
            this.BinaryData = null;

            base.CreateMode();
        }

        public override void ModifyMode(JryCover source)
        {
            this.DoubanId = source.DoubanId;
            this.Uri = source.Uri;
            this.BinaryData = source.BinaryData;

            base.ModifyMode(source);
        }

        public string DoubanId
        {
            get { return this.doubanId; }
            set { this.SetPropertyRef(ref this.doubanId, value); }
        }

        public string Uri
        {
            get { return this.uri; }
            set { this.SetPropertyRef(ref this.uri, value); }
        }

        /// <summary>
        /// can be null.
        /// </summary>
        public byte[] BinaryData
        {
            get { return this.binaryData; }
            set
            {
                if (this.SetPropertyRef(ref this.binaryData, value))
                    this.OnUpdatedBinaryData();
            }
        }

        public JryCoverSourceType CoverSourceType { get; set; }

        public ImageViewModel ImageViewModel
        {
            get { return this.imageViewModel; }
            private set { this.SetPropertyRef(ref this.imageViewModel, value); }
        }

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

        public void SaveToObject(JryCover commitObject)
        {
            commitObject.CoverSourceType = this.CoverSourceType;
            commitObject.DoubanId = this.DoubanId;
            commitObject.Uri = this.Uri;
            commitObject.BinaryData = this.BinaryData;
        }

        public async Task<bool> CommitAsync()
        {
            var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;

            var obj = this.GetCommitObject();

            this.SaveToObject(obj);

            if (this.Action == ObjectChangedAction.Create)
                obj.BuildMetaData();

            return await this.CommitAsync(coverManager, obj);
        }
    }
}