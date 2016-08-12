using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace JryVideo.Model
{
    public sealed partial class JryVideo : VideoInfoAttached, IJryChild<JryEntity>
    {
        public JryVideo()
        {
            this.Entities = new List<JryEntity>();
        }

        [NotNull]
        public List<JryEntity> Entities { get; set; }

        List<JryEntity> IJryChild<JryEntity>.Childs => this.Entities;

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.Entities == null)
            {
                throw new ArgumentException();
            }

            return false;
        }

        public override void CheckError()
        {
            base.CheckError();
            DataCheck.NotNull(this.Entities);
        }
    }
}