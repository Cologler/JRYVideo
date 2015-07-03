using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace JryVideo.Model
{
    public class JryArtist : JryObject, IEquatable<JryArtist>, IJryCoverParent
    {
        public JryArtist()
        {
            this.Names = new List<string>();
        }

        /// <summary>
        /// may null.
        /// </summary>
        public string DoubanId { get; set; }

        public List<string> Names { get; set; }

        /// <summary>
        /// may null.
        /// </summary>
        public string Description { get; set; }

        public string CoverId { get; set; }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.Names == null)
            {
                throw new ArgumentException();
            }

            return false;
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
        public override int GetHashCode()
        {
            return (this.Id != null ? this.Id.GetHashCode() : 0);
        }

        public static bool operator ==(JryArtist left, JryArtist right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(JryArtist left, JryArtist right)
        {
            return !Equals(left, right);
        }
    }
}