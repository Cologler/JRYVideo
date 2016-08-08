using System;
using System.Enums;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Jasily.ComponentModel.Editable;
using Jasily.Net;
using JryVideo.Common;
using JryVideo.Core.Douban;
using JryVideo.Core.Managers;
using JryVideo.Model;
using JryVideo.Model.Interfaces;
using JryVideo.Selectors.WebImageSelector;

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
        private string coverId;

        private CoverEditorViewModel()
        {
            
        }
        
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

        public string DoubanId
        {
            get { return this.doubanId; }
            set { this.SetPropertyRef(ref this.doubanId, value); }
        }

        public string ImdbId
        {
            get { return this.imdbId; }
            set { this.SetPropertyRef(ref this.imdbId, value); }
        }

        public ImageViewModel ImageViewModel
        {
            get { return this.imageViewModel; }
            private set { this.SetPropertyRef(ref this.imageViewModel, value); }
        }

        private void OnUpdatedBinaryData()
        {
            this.ImageViewModel = this.BinaryData == null ? null : ImageViewModel.Build(this.BinaryData);
        }

        public Task<bool> LoadFromUrlAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Uri)) return Task.FromResult(false);
            var request = WebRequest.CreateHttp(this.Uri);
            return this.LoadFromRequestAsync(request);
        }

        private async Task<bool> LoadFromRequestAsync(HttpWebRequest request)
        {
            var result = await request.GetResultAsBytesAsync();
            this.BinaryData = result.Result;
            return this.BinaryData != null;
        }

        public async Task<bool> LoadFromDoubanAsync()
        {
            if (string.IsNullOrWhiteSpace(this.DoubanId)) return false;
            var json = await DoubanHelper.TryGetMovieInfoAsync(this.DoubanId);
            if (json == null) return false;
            var requests = json.GetMovieCoverRequest().ToArray();
            foreach (var request in requests)
            {
                if (await this.LoadFromRequestAsync(request)) return true;
            }
            return false;
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
            var coverManager = this.GetManagers().CoverManager;
            var obj = this.GetCommitObject();
            if (this.Action == ObjectChangedAction.Modify && !this.isChanged)
            {
                return obj;
            }
            obj.Id = this.coverId;
            obj.CoverType = CoverType.Video;
            this.WriteToObject(obj);
            return await this.CommitAsync(coverManager, obj);
        }

        public static CoverEditorViewModel From(JryCover cover)
        {
            if (cover == null) throw new ArgumentNullException(nameof(cover));
            var viewModel = new CoverEditorViewModel();
            viewModel.ModifyMode(cover);
            return viewModel;
        }

        public static async Task<CoverEditorViewModel> FromAsync(CoverManager manager, ICoverParent coverParent)
        {
            var viewModel = new CoverEditorViewModel
            {
                coverId = coverParent.CoverId
            };
            if (viewModel.coverId == null) // since cover id equal id, new object may contain null cover id.
            {
                throw new ArgumentNullException();
            }
            else
            {
                var cover = await manager.FindAsync(viewModel.coverId);
                if (cover == null)
                {
                    viewModel.CreateMode();
                }
                else
                {
                    viewModel.ModifyMode(cover);
                }
            }
            return viewModel;
        }
    }
}