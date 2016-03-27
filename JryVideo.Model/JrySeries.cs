using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JryVideo.Model
{
    public sealed class JrySeries : JryObject, IJryChild<JryVideoInfo>, INameable, IImdbItem, ITheTVDBItem, ITagable
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

            this.Tags = CombineStrings(this.Tags, other.Tags);

            if (!CanCombineField(this.ImdbId, other.ImdbId)) throw new InvalidOperationException();
            this.ImdbId = this.ImdbId ?? other.ImdbId;

            if (!CanCombineField(this.TheTVDBId, other.TheTVDBId)) throw new InvalidOperationException();
            this.TheTVDBId = this.TheTVDBId ?? other.TheTVDBId;
        }

        public override void Saving()
        {
            base.Saving();

            if (this.Tags?.Count == 0) this.Tags = null;
        }

        public struct QueryParameter
        {
            public string OriginText { get; }

            public QueryMode Mode { get; }

            public string Keyword { get; }

            public QueryParameter([CanBeNull] string originText, QueryMode mode, [CanBeNull] string value)
            {
                this.OriginText = originText;
                this.Mode = mode;
                this.Keyword = value;
            }
        }

        public enum QueryMode
        {
            Any,

            OriginText,

            SeriesId,

            VideoId,

            EntityId,

            DoubanId,

            Tag,

            VideoType,

            VideoYear,

            ImdbId,
        }
    }
}