using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JryVideo.Model
{
    public sealed class JrySeries : JryObject, IJryChild<JryVideoInfo>, INameable, IImdbItem
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
    }
}