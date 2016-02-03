using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JryVideo.Model
{
    public sealed class JrySeries : JryObject, IJryChild<JryVideoInfo>, INameable, IImdbItem, ITheTVDBItem
    {
        public JrySeries()
        {
            this.Names = new List<string>();
            this.Videos = new List<JryVideoInfo>();
        }

        [ItemNotNull]
        [NotNull]
        public List<string> Names { get; set; }

        public string GetMajorName() => this.Names[0];

        [ItemNotNull]
        [NotNull]
        public List<JryVideoInfo> Videos { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<string> Tags { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string ImdbId { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string TheTVDBId { get; set; }

        List<JryVideoInfo> IJryChild<JryVideoInfo>.Childs => this.Videos;

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.Names == null || this.Videos == null)
            {
                throw new ArgumentException();
            }

            if (this.Names.Count == 0)
            {
                JasilyLogger.Current.WriteLine<JrySeries>(JasilyLogger.LoggerMode.Debug, "series name can not be empty.");
                return true;
            }

            return false;
        }

        public void CombineFrom(JrySeries other)
        {
            this.Names = this.Names.Concat(other.Names).Distinct().ToList();
            this.Videos = this.Videos.Concat(other.Videos).ToList();
            other.Videos = new List<JryVideoInfo>();

            if (this.Tags == null)
            {
                this.Tags = other.Tags;
            }
            else if (other.Tags != null)
            {
                this.Tags = this.Tags.Concat(other.Tags).Distinct().ToList();
            }

            if (!CombineEquals(this.ImdbId, other.ImdbId)) throw new InvalidOperationException();
            this.ImdbId = this.ImdbId ?? other.ImdbId;

            if (!CombineEquals(this.TheTVDBId, other.TheTVDBId)) throw new InvalidOperationException();
            this.TheTVDBId = this.TheTVDBId ?? other.TheTVDBId;
        }
    }
}