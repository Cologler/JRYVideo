using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed class VideoRoleCollection : VideoInfoAttached
    {
        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<VideoRole> MajorRoles { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<VideoRole> MinorRoles { get; set; }

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

        private static void Combine(ICollection<VideoRole> to, IEnumerable<VideoRole> from)
        {
            foreach (var role in from)
            {
                var same = to.FirstOrDefault(z => z.ActorId == role.ActorId);
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

        public IEnumerable<VideoRole> Roles()
        {
            if (this.MajorRoles != null)
            {
                foreach (var role in this.MajorRoles)
                {
                    yield return role;
                }
            }

            if (this.MinorRoles != null)
            {
                foreach (var role in this.MinorRoles)
                {
                    yield return role;
                }
            }
        }
    }
}