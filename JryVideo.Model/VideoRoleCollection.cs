using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace JryVideo.Model
{
    public sealed class VideoRoleCollection : VideoInfoAttached
    {
        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<JryVideoRole> MajorRoles { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<JryVideoRole> MinorRoles { get; set; }
    }
}