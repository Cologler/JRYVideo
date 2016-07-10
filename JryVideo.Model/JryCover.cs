using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed partial class JryCover : RootObject
    {
        public CoverType CoverType { get; set; }

        [BsonIgnoreIfDefault]
        public byte[] BinaryData { get; set; }

        /// <summary>
        /// (value / 10) was opacity.
        /// </summary>
        [BsonIgnoreIfDefault]
        public int? Opacity { get; set; }

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

        public struct QueryParameter
        {
            public string Id { get; set; }
        }
    }
}