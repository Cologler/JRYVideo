using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JryVideo.Model
{
    public sealed class JryVideoInfo : JryObject, IJryCoverParent, INameable, IImdbItem
    {
        public JryVideoInfo()
        {
            this.Names = new List<string>();
        }

        public string Type { get; set; }

        public int Year { get; set; }

        public int Index { get; set; }

        [CanBeNull]
        [ItemNotNull]
        [BsonIgnoreIfDefault]
        [BsonElement("ArtistIds")]
        public List<JryVideoRole> Roles { get; set; }

        [NotNull]
        [ItemNotNull]
        public List<string> Names { get; set; }

        public string GetMajorName() => this.Names.FirstOrDefault();

        [BsonIgnoreIfDefault]
        public string LastVideoId { get; set; }

        [BsonIgnoreIfDefault]
        public string NextVideoId { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string DoubanId { get; set; }

        /// <summary>
        /// 是否正在追剧
        /// </summary>
        [BsonIgnoreIfDefault]
        public bool IsTracking { get; set; }

        [BsonIgnoreIfDefault]
        public bool IsAllAired { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string ImdbId { get; set; }

        [BsonIgnoreIfDefault]
        public int EpisodesCount { get; set; }

        [CanBeNull]
        [ItemNotNull]
        [BsonIgnoreIfDefault]
        public List<string> Tags { get; set; }

        [BsonIgnoreIfDefault]
        public int? EpisodeOffset { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string CoverId { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string BackgroundImageId { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public DayOfWeek? DayOfWeek { get; set; }

        /// <summary>
        /// may not DayOfWeek (utc)
        /// </summary>
        [CanBeNull]
        [BsonIgnoreIfDefault]
        public DateTime? StartDate { get; set; }

        [CanBeNull]
        [BsonIgnore]
        public DateTime? StartLocalDate
        {
            get { return this.StartDate?.ToLocalTime(); }
            set { this.StartDate = value?.ToUniversalTime(); }
        }

        public int GetTodayEpisode(DateTime dt)
        {
            if (this.StartDate == null || dt < this.StartDate.Value)
                return 0;

            return Convert.ToInt32((dt - this.StartDate.Value).TotalDays / 7) + 1;
        }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.Names == null)
            {
                throw new ArgumentException();
            }

            if (this.Type.IsNullOrWhiteSpace())
            {
                JasilyLogger.Current.WriteLine<JryVideoInfo>(JasilyLogger.LoggerMode.Release, "video type can not be empty.");
                return true;
            }

            if (!IsYearValid(this.Year))
            {
                JasilyLogger.Current.WriteLine<JryVideoInfo>(JasilyLogger.LoggerMode.Release, "video year was invalid.");
                return true;
            }

            if (!IsIndexValid(this.Index))
            {
                JasilyLogger.Current.WriteLine<JryVideoInfo>(JasilyLogger.LoggerMode.Release, "video index was invalid.");
                return true;
            }

            if (!IsEpisodesCountValid(this.EpisodesCount))
            {
                JasilyLogger.Current.WriteLine<JryVideoInfo>(JasilyLogger.LoggerMode.Release, "video episodes count was invalid.");
                return true;
            }

            return false;
        }

        public static bool IsYearValid(int year) => year < 2100 && year > 1900;

        public static bool IsIndexValid(int index) => index > 0;

        public static bool IsEpisodesCountValid(int episodesCount) => episodesCount >= 0;
    }
}