﻿using System;
using System.Linq;
using Jasily.ComponentModel.Editable;
using JryVideo.Common;
using JryVideo.Model;
using JryVideo.Model.Interfaces;

namespace JryVideo.Editors.RoleEditor
{
    public class RoleEditorViewModel : EditorItemViewModel<VideoRole>
    {
        private string roleName;
        private ImageViewModel imageViewModel;
        private string actorName;

        [EditableField]
        public string ActorName
        {
            get { return this.actorName; }
            set { this.SetPropertyRef(ref this.actorName, value); }
        }

        public string RoleName
        {
            get { return this.roleName; }
            set { this.SetPropertyRef(ref this.roleName, value); }
        }

        public ImageViewModel ImageViewModel
        {
            get { return this.imageViewModel; }
            private set { this.SetPropertyRef(ref this.imageViewModel, value); }
        }

        public override async void ReadFromObject(VideoRole obj)
        {
            base.ReadFromObject(obj);

            this.RoleName = obj.RoleName == null || obj.RoleName.Count == 0 ? string.Empty : obj.RoleName.AsLines();

            var coverId = (obj as ICoverParent).CoverId;
            if (coverId != null)
            {
                var cover = await this.GetManagers().CoverManager.FindAsync(coverId);
                if (cover != null)
                {
                    this.ImageViewModel = ImageViewModel.Build(cover.BinaryData);
                }
            }
        }

        public override void WriteToObject(VideoRole obj)
        {
            base.WriteToObject(obj);

            if (this.RoleName.IsNullOrWhiteSpace())
            {
                obj.RoleName = null;
            }
            else
            {
                obj.RoleName = this.RoleName.AsLines()
                    .Where(z => !z.IsNullOrWhiteSpace())
                    .Select(z => z.Trim())
                    .Distinct()
                    .ToList();
                if (obj.RoleName.Count == 0)
                {
                    obj.RoleName = null;
                }
            }
        }
    }
}