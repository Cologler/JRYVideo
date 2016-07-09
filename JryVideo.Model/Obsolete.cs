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
        public string CoverId { get; set; }

        [Obsolete]
        [BsonIgnoreIfDefault]
        public int Version { get; set; }
    }
}