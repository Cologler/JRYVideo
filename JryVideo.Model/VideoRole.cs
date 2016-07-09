using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using JryVideo.Model.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed partial class VideoRole : JryObject, IJasilyLoggerObject<VideoRole>, IEquatable<VideoRole>, ICoverParent, INameable
    {
        public string ActorId { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<string> RoleName { get; set; }

        public bool Equals(VideoRole other)
        {
            if (other == null) return false;
            return this.ActorId == other.ActorId;
        }

        [CanBeNull]
        [BsonIgnore]
        List<string> INameable.Names
        {
            get { return this.RoleName; }
            set { this.RoleName = value; }
        }

        CoverType ICoverParent.CoverType => CoverType.Role;

        string ICoverParent.CoverId
        {
            get { return this.Id; }
            set { throw new NotSupportedException(); }
        }

        public string GetMajorName() => this.RoleName?.FirstOrDefault();

        public void CombineFrom(VideoRole other)
        {
            if (this.RoleName == null)
            {
                this.RoleName = other.RoleName;
            }
            else if (other.RoleName != null)
            {
                this.RoleName = this.RoleName.Concat(other.RoleName).Distinct().ToList();
            }
        }

        public static VideoRole Create()
        {
            var role = new VideoRole();
            role.BuildMetaData();
            return role;
        }
    }
}