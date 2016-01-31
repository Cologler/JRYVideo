using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public class JryArtist : JryObject, IEquatable<JryArtist>, IJryCoverParent, INameable
    {
        public JryArtist()
        {
            this.Names = new List<string>();
        }

        public ArtistType Type { get; set; }

        /// <summary>
        /// must contain a name
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public List<string> Names { get; set; }

        /// <summary>
        /// may null.
        /// </summary>
        [BsonIgnoreIfDefault]
        public string Description { get; set; }

        [BsonIgnoreIfDefault]
        public string CoverId { get; set; }

        [BsonIgnoreIfDefault]
        public string TheTVDBId { get; set; }

        /// <summary>
        /// may null.
        /// </summary>
        [BsonIgnoreIfDefault]
        public string DoubanId { get; set; }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.Names == null)
            {
                throw new ArgumentException();
            }

            return this.Names.Count == 0;
        }

        /// <summary>
        /// 确定指定的 <see cref="T:System.Object"/> 是否等于当前的 <see cref="T:System.Object"/>。
        /// </summary>
        /// <returns>
        /// 如果指定的对象等于当前对象，则为 true；否则为 false。
        /// </returns>
        /// <param name="obj">要与当前对象进行比较的对象。</param>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as JryArtist);
        }

        /// <summary>
        /// 指示当前对象是否等于同一类型的另一个对象。
        /// </summary>
        /// <returns>
        /// 如果当前对象等于 <paramref name="other"/> 参数，则为 true；否则为 false。
        /// </returns>
        /// <param name="other">与此对象进行比较的对象。</param>
        public bool Equals(JryArtist other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || string.Equals(this.Id, other.Id));
        }

        /// <summary>
        /// 用作特定类型的哈希函数。
        /// </summary>
        /// <returns>
        /// 当前 <see cref="T:System.Object"/> 的哈希代码。
        /// </returns>
        public override int GetHashCode() => this.Id?.GetHashCode() ?? 0;

        public static bool operator ==(JryArtist left, JryArtist right) => Equals(left, right);

        public static bool operator !=(JryArtist left, JryArtist right) => !Equals(left, right);

        public struct QueryParameter
        {
            public string TheTVDBId { get; set; }

            public string DoubanId { get; set; }
        }
    }
}