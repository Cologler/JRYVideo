using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public partial class JryVideoInfo
    {
        [BsonIgnoreIfDefault]
        [BsonElement("ArtistIds")]
        [Obsolete]
        public List<JryVideoRole> Roles { get; set; }

        [Obsolete]
        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string CoverId { get; set; }
    }
}