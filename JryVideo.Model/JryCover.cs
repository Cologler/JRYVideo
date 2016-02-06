using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Diagnostics;

namespace JryVideo.Model
{
    public sealed class JryCover : JryObject,
        JryCover.ISeriesRoleCover,
        JryCover.IVideoRoleCover
    {
        public JryCoverType CoverType { get; set; }

        public JryCoverSourceType CoverSourceType { get; set; }

        [BsonIgnoreIfDefault]
        public string DoubanId { get; set; }

        [BsonIgnoreIfDefault]
        public string ImdbId { get; set; }

        [BsonIgnoreIfDefault]
        public string VideoId { get; set; }

        [BsonIgnoreIfDefault]
        public string SeriesId { get; set; }

        [BsonIgnoreIfDefault]
        public string ActorId { get; set; }

        [BsonIgnoreIfDefault]
        public string Uri { get; set; }

        [BsonIgnoreIfDefault]
        public byte[] BinaryData { get; set; }

        public string GetDownloadId()
        {
            var key = ((int)this.CoverType) + "_";

            switch (this.CoverType)
            {
                case JryCoverType.Role:
                    return key + (this.VideoId ?? this.SeriesId) + "_" + this.ActorId;

                case JryCoverType.Background:
                case JryCoverType.Video:
                    return key + this.VideoId;

                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        public struct QueryParameter
        {
            public JryCoverType CoverType { get; set; }

            public string VideoId { get; set; }

            public string SeriesId { get; set; }
        }

        public interface ICover
        {
            JryCoverType CoverType { get; set; }

            JryCoverSourceType CoverSourceType { get; set; }

            byte[] BinaryData { get; set; }
        }

        public interface IRoleCover : ICover
        {
            string ActorId { get; set; }
        }

        public interface IVideoRoleCover : IRoleCover
        {
            string VideoId { get; set; }
        }

        public interface ISeriesRoleCover : IRoleCover
        {
            string SeriesId { get; set; }
        }
    }
}