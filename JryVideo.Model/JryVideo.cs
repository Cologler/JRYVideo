using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed class JryVideo : VideoInfoAttached, IJryChild<JryEntity>
    {
        public JryVideo()
        {
            this.Entities = new List<JryEntity>();
        }

        [NotNull]
        public List<JryEntity> Entities { get; set; }

        /// <summary>
        /// 尽量排序，但是不一定排序
        /// </summary>
        [BsonIgnoreIfDefault]
        public List<int> Watcheds { get; set; }

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
            DataChecker.NotNull(this.Entities);
        }
    }
}