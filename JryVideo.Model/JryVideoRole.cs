using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Diagnostics;

namespace JryVideo.Model
{
    public sealed class JryVideoRole : JryInfo, IJasilyLoggerObject<JryVideoRole>, IEquatable<JryVideoRole>, IJryCoverParent
    {
        [BsonIgnore]
        public string ArtistId => this.Id;

        /// <summary>
        /// only the firse name from artist, as cache
        /// </summary>
        public string ActorName { get; set; }

        public string RoleName { get; set; }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError())
                return true;

            if (this.ActorName.IsNullOrWhiteSpace())
            {
                this.Log(JasilyLogger.LoggerMode.Debug, "name can not be empty.");
                return true;
            }

            return false;
        }

        public bool Equals(JryVideoRole other)
        {
            if (other == null) return false;
            return this.ArtistId == other.ArtistId;
        }

        public string CoverId { get; set; }
    }
}