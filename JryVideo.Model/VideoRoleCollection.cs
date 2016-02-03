using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Linq;

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

        public struct QueryParameter
        {
            public string ActorId { get; set; }
        }

        public void CombineFrom(VideoRoleCollection other)
        {
            if (this.MajorRoles == null)
            {
                this.MajorRoles = other.MajorRoles;
            }
            else if (other.MajorRoles != null)
            {
                Combine(this.MajorRoles, other.MajorRoles);
            }

            if (this.MinorRoles == null)
            {
                this.MinorRoles = other.MinorRoles;
            }
            else if (other.MinorRoles != null)
            {
                Combine(this.MinorRoles, other.MinorRoles);
            }
        }

        private static void Combine(ICollection<JryVideoRole> to, IEnumerable<JryVideoRole> from)
        {
            foreach (var role in from)
            {
                var same = to.FirstOrDefault(z => z.ArtistId == role.ArtistId);
                if (same == null)
                {
                    to.Add(role);
                }
                else
                {
                    same.CombineFrom(role);
                }
            }
        }
    }
}