using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Attributes;
using System.Diagnostics;

namespace JryVideo.Model
{
    public sealed class JryCover : JryObject, ICloneable<JryCover>
    {
        [Cloneable]
        public JryCoverType CoverType { get; set; }

        [Cloneable]
        public JryCoverSourceType CoverSourceType { get; set; }

        [Cloneable]
        [BsonIgnoreIfDefault]
        public string DoubanId { get; set; }

        [Cloneable]
        [BsonIgnoreIfDefault]
        public string ImdbId { get; set; }

        /// <summary>
        /// {videoId}_{actorId}
        /// </summary>
        [BsonIgnoreIfDefault]
        public string RoleId { get; set; }

        [Cloneable]
        public string Uri { get; set; }

        [Cloneable]
        public byte[] BinaryData { get; set; }

        public string GetDownloadId()
        {
            var key = ((int)this.CoverType) + "_";

            if (this.RoleId != null) return key + this.RoleId;

            switch (this.CoverSourceType)
            {
                case JryCoverSourceType.Local:
                    throw new ArgumentException();

                case JryCoverSourceType.Uri:
                    key += this.Uri;
                    break;

                case JryCoverSourceType.Douban:
                    key += this.DoubanId;
                    break;

                case JryCoverSourceType.Imdb:
                    key += this.ImdbId;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return key;
        }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.BinaryData == null || this.BinaryData.Length == 0)
            {
                JasilyLogger.Current.WriteLine<JryCover>(JasilyLogger.LoggerMode.Debug, "cover data can not be empty.");
                return true;
            }

            return false;
        }

        public JryCover Clone()
        {
            return CloneableAttribute.Clone(this, new JryCover());
        }

        /// <summary>
        /// 创建作为当前实例副本的新对象。
        /// </summary>
        /// <returns>
        /// 作为此实例副本的新对象。
        /// </returns>
        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}