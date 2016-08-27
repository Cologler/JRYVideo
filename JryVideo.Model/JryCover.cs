using System;
using System.Diagnostics;
using System.IO;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed partial class JryCover : RootObject
    {
        public CoverType CoverType { get; set; }

        [BsonIgnoreIfDefault]
        public byte[] BinaryData { get; set; }

        [BsonIgnore]
        public Stream BinaryStream => this.BinaryData.ToMemoryStream();

        /// <summary>
        /// (value / 10) was opacity.
        /// </summary>
        [BsonIgnoreIfDefault]
        public int? Opacity { get; set; }

        public override void CheckError()
        {
            base.CheckError();
            DataCheck.NotNull(this.BinaryData);
            DataCheck.False(this.BinaryData.Length == 0);
        }
    }
}