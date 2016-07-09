﻿using System;
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
}