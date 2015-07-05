using System;

namespace JryVideo.Model
{
    public sealed class JryFormat : IEquatable<JryFormat>
    {
        public JryFormatType Type { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// 指示当前对象是否等于同一类型的另一个对象。
        /// </summary>
        /// <returns>
        /// 如果当前对象等于 <paramref name="other"/> 参数，则为 true；否则为 false。
        /// </returns>
        /// <param name="other">与此对象进行比较的对象。</param>
        public bool Equals(JryFormat other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Type == other.Type && string.Equals(this.Value, other.Value);
        }

        /// <summary>
        /// 用作特定类型的哈希函数。
        /// </summary>
        /// <returns>
        /// 当前 <see cref="T:System.Object"/> 的哈希代码。
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) this.Type * 397) ^ (this.Value != null ? this.Value.GetHashCode() : 0);
            }
        }

        public static bool operator ==(JryFormat left, JryFormat right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(JryFormat left, JryFormat right)
        {
            return !Equals(left, right);
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
            return base.Equals(obj);
        }
    }
}