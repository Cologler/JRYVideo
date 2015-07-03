using System;
using System.Attributes;
using System.Collections.Generic;
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
        public string DoubanId { get; set; }

        [Cloneable]
        public string Uri { get; set; }

        [Cloneable]
        public byte[] BinaryData { get; set; }

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