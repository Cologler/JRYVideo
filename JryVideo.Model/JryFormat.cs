using System;

namespace JryVideo.Model
{
    public sealed class JryFormat : IEquatable<JryFormat>
    {
        public JryFormatType Type { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// ָʾ��ǰ�����Ƿ����ͬһ���͵���һ������
        /// </summary>
        /// <returns>
        /// �����ǰ������� <paramref name="other"/> ��������Ϊ true������Ϊ false��
        /// </returns>
        /// <param name="other">��˶�����бȽϵĶ���</param>
        public bool Equals(JryFormat other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Type == other.Type && string.Equals(this.Value, other.Value);
        }

        /// <summary>
        /// �����ض����͵Ĺ�ϣ������
        /// </summary>
        /// <returns>
        /// ��ǰ <see cref="T:System.Object"/> �Ĺ�ϣ���롣
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
        /// ȷ��ָ���� <see cref="T:System.Object"/> �Ƿ���ڵ�ǰ�� <see cref="T:System.Object"/>��
        /// </summary>
        /// <returns>
        /// ���ָ���Ķ�����ڵ�ǰ������Ϊ true������Ϊ false��
        /// </returns>
        /// <param name="obj">Ҫ�뵱ǰ������бȽϵĶ���</param>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}