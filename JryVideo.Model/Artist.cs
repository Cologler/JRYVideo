using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public class Artist : JryObject, IEquatable<Artist>, IJryCoverParent, INameable, ITheTVDBItem,
        IImdbItem
    {
        public Artist()
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

        public string GetMajorName() => this.Names[0];

        [BsonIgnoreIfDefault]
        public string CoverId { get; set; }

        JryCoverType IJryCoverParent.CoverType => JryCoverType.Artist;

        #region remote id

        [BsonIgnoreIfDefault]
        public string TheTVDBId { get; set; }

        /// <summary>
        /// may null.
        /// </summary>
        [BsonIgnoreIfDefault]
        public string DoubanId { get; set; }

        [BsonIgnoreIfDefault]
        public string ImdbId { get; set; }

        #endregion

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
            return this.Equals(obj as Artist);
        }

        /// <summary>
        /// 指示当前对象是否等于同一类型的另一个对象。
        /// </summary>
        /// <returns>
        /// 如果当前对象等于 <paramref name="other"/> 参数，则为 true；否则为 false。
        /// </returns>
        /// <param name="other">与此对象进行比较的对象。</param>
        public bool Equals(Artist other)
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

        public static bool operator ==(Artist left, Artist right) => Equals(left, right);

        public static bool operator !=(Artist left, Artist right) => !Equals(left, right);

        public struct QueryParameter
        {
            public string TheTVDBId { get; set; }

            public string DoubanId { get; set; }
        }

        public override void Saving()
        {
            base.Saving();

            if (this.TheTVDBId.IsNullOrWhiteSpace()) this.TheTVDBId = null;
            if (this.DoubanId.IsNullOrWhiteSpace()) this.DoubanId = null;
            if (this.ImdbId.IsNullOrWhiteSpace()) this.ImdbId = null;
        }
    }
}