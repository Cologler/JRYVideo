using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public sealed class JryVideo : JryObject, IJryChild<JryEntity>
    {
        public JryVideo()
        {
            this.Entities = new List<JryEntity>();
        }

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

        /// <summary>
        /// Guid.NewGuid().ToString().ToUpper()
        /// </summary>
        /// <returns></returns>
        protected override string BuildId()
        {
            return this.Id;
        }

        public static JryVideo Build(JryVideoInfo info)
        {
            return new JryVideo()
            {
                Id = info.Id
            };
        }
    }
}