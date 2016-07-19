using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public partial class JryVideoInfo
    {
        [Obsolete]
        [BsonIgnoreIfDefault]
        [BsonElement("ArtistIds")]
        public List<VideoRole> Roles { get; set; }

        [Obsolete]
        [CanBeNull]
        [BsonIgnoreIfDefault]
        [BsonElement("CoverId")]
        public string CoverId { get; set; }

        [Obsolete]
        [BsonIgnoreIfDefault]
        [BsonElement("Version")]
        public int Version { get; set; }

        [Obsolete]
        [CanBeNull]
        [BsonIgnoreIfDefault]
        [BsonElement("BackgroundImageId")]
        public string BackgroundImageId { get; set; }
    }

    public partial class VideoRole
    {
        [Obsolete]
        [BsonIgnoreIfDefault]
        [BsonElement("ActorName")]
        public string ActorName { get; set; }

        [Obsolete]
        [BsonIgnoreIfDefault]
        [BsonElement("CoverId")]
        public string CoverId { get; set; }
    }

    public partial class Artist
    {
        [Obsolete]
        [BsonIgnoreIfDefault]
        [BsonElement("CoverId")]
        public string CoverId { get; set; }
    }

    public partial class JryCover
    {
        [Obsolete]
        [BsonIgnoreIfDefault]
        public JryCoverSourceType CoverSourceType { get; set; }

        [Obsolete]
        [BsonIgnoreIfDefault]
        public string DoubanId { get; set; }

        [Obsolete]
        [BsonIgnoreIfDefault]
        public string ImdbId { get; set; }

        [Obsolete]
        [BsonIgnoreIfDefault]
        public string VideoId { get; set; }

        [Obsolete]
        [BsonIgnoreIfDefault]
        public string SeriesId { get; set; }

        [Obsolete]
        [BsonIgnoreIfDefault]
        public string ActorId { get; set; }

        [Obsolete]
        [BsonIgnoreIfDefault]
        public string Uri { get; set; }
    }

    public partial class JryVideo
    {
        /// <summary>
        /// 尽量排序，但是不一定排序
        /// </summary>
        [Obsolete]
        [BsonIgnoreIfDefault]
        public List<int> Watcheds { get; set; }
    }
}