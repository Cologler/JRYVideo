using System;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public sealed class JryCover : JryObject, IInitializable<JryCover>
    {
        public JryCoverType CoverType { get; set; }

        public string DoubanId { get; set; }

        public string Uri { get; set; }

        public byte[] BinaryData { get; set; }

        public JryCover InitializeInstance(JryCover obj)
        {
            return base.InitializeInstance(obj);
        }

        public override IEnumerable<string> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.BinaryData == null || this.BinaryData.Length == 0)
            {
                yield return "error BinaryData";
            }
        }
    }
}