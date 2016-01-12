using Jasily.ComponentModel;
using Jasily.Net;
using JryVideo.Common;
using JryVideo.Core;
using JryVideo.Core.Douban;
using JryVideo.Model;
using JryVideo.Selectors.WebImageSelector;
using System.Enums;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace JryVideo.Editors.CoverEditor
{
    public sealed class CoverEditorViewModel : EditorItemViewModel<JryCover>
    {
        private string doubanId;
        private string imdbId;
        private string uri;
        private byte[] binaryData;
        private ImageViewModel imageViewModel;
        private bool isChanged;

        [EditableField]
        public string DoubanId
        {
            get { return this.doubanId; }
            set { this.SetPropertyRef(ref this.doubanId, value); }
        }

        [EditableField]
        public string Uri
        {
            get { return this.uri; }
            set { this.SetPropertyRef(ref this.uri, value); }
        }

        /// <summary>
        /// can be null.
        /// </summary>
        [EditableField]
        public byte[] BinaryData
        {
            get { return this.binaryData; }
            set
            {
                if (this.SetPropertyRef(ref this.binaryData, value))
                {
                    this.isChanged = true;
                    this.OnUpdatedBinaryData();
                }
            }
        }

        [EditableField]
        public string ImdbId
        {
            get { return this.imdbId; }
            set { this.SetPropertyRef(ref this.imdbId, value); }
        }

        [EditableField]
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
            this.BinaryData = result.Result;
            return this.BinaryData != null;
        }

        public async Task<bool> LoadFromDoubanAsync()
        {
            if (string.IsNullOrWhiteSpace(this.DoubanId)) return false;
            var json = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);
            if (json?.Images?.Large == null) return false;
            this.Uri = json.GetLargeImageUrl();
            return await this.LoadFromUrlAsync();
        }

        /// <summary>
        /// null mean no select.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public async Task<bool?> LoadFromImdbIdAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(this.ImdbId)) return false;
            var url = WebImageSelectorWindow.StartSelectByImdbId(window, this.ImdbId);
            if (string.IsNullOrWhiteSpace(url)) return null;
            this.Uri = url;
            return await this.LoadFromUrlAsync();
        }

        public async Task<JryCover> CommitAsync()
        {
            var coverManager = JryVideoCore.Current.CurrentDataCenter.CoverManager;

            var obj = this.GetCommitObject();

            if (this.Action == ObjectChangedAction.Modify && !this.isChanged)
            {
                return obj;
            }

            this.WriteToObject(obj);

            if (this.Action == ObjectChangedAction.Create)
                obj.BuildMetaData();

            return await this.CommitAsync(coverManager, obj);
        }
    }
}