using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using JryVideo.Model.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed class JryVideoRole : JryInfo, IJasilyLoggerObject<JryVideoRole>, IEquatable<JryVideoRole>, ICoverParent, INameable
    {
        [BsonIgnore]
        public string ArtistId => this.Id;

        #region obsolete

        [Obsolete]
        [BsonIgnoreIfDefault]
        [BsonElement("ActorName")]
        public string ActorName { get; set; }

        #endregion

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<string> RoleName { get; set; }

        public bool Equals(JryVideoRole other)
        {
            if (other == null) return false;
            return this.ArtistId == other.ArtistId;
        }

        [BsonIgnoreIfDefault]
        public string CoverId { get; set; }

        [CanBeNull]
        [BsonIgnore]
        List<string> INameable.Names
        {
            get { return this.RoleName; }
            set { this.RoleName = value; }
        }

        JryCoverType ICoverParent.CoverType => JryCoverType.Role;

        public string GetMajorName() => this.RoleName?.FirstOrDefault();

        public override void Saving()
        {
            base.Saving();
#pragma warning disable 612
            this.ActorName = null;
#pragma warning restore 612
        }

        public void CombineFrom(JryVideoRole other)
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
    }
}