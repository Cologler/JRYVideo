using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JryVideo.Model
{
    public sealed class JryVideoInfo : JryObject, IJryCoverParent
    {
        public JryVideoInfo()
        {
            this.Names = new List<string>();
            this.Tags = new List<string>();
            this.Roles = new List<JryVideoRole>();
        }

        public string Type { get; set; }

        public int Year { get; set; }

        public int Index { get; set; }

        [BsonElement("ArtistIds")]
        public List<JryVideoRole> Roles { get; set; }

        public List<string> Names { get; set; }

        [BsonIgnoreIfDefault]
        public string DoubanId { get; set; }

        /// <summary>
        /// 是否正在追剧
        /// </summary>
        [BsonIgnoreIfDefault]
        public bool IsTracking { get; set; }

        [BsonIgnoreIfDefault]
        public string ImdbId { get; set; }

        [BsonIgnoreIfDefault]
        public int EpisodesCount { get; set; }

        public List<string> Tags { get; set; }

        [BsonIgnoreIfDefault]
        public int? EpisodeOffset { get; set; }

        [BsonIgnoreIfDefault]
        public string CoverId { get; set; }

        [BsonIgnoreIfDefault]
        public string BackgroundImageId { get; set; }

        [BsonIgnoreIfDefault]
        public DayOfWeek? DayOfWeek { get; set; }

        /// <summary>
        /// may not DayOfWeek (utc)
        /// </summary>
        [BsonIgnoreIfDefault]
        public DateTime? StartDate { get; set; }

        [BsonIgnore]
        public DateTime? StartLocalDate
        {
            get { return this.StartDate?.ToLocalTime(); }
            set { this.StartDate = value?.ToUniversalTime(); }
        }

        public int GetTodayEpisode(DateTime dt)
        {
            if (this.DayOfWeek == null ||
                this.StartDate == null ||
                dt.DayOfWeek != this.DayOfWeek ||
                dt < this.StartDate.Value)
                return 0;

            return Convert.ToInt32((dt - this.StartDate.Value).TotalDays / 7) + 1;
        }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.Names == null || this.Tags == null || this.Roles == null)
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

        public static bool IsYearValid(int year)
        {
            return year < 2100 && year > 1900;
        }

        public static bool IsIndexValid(int index)
        {
            return index > 0;
        }

        public static bool IsEpisodesCountValid(int episodesCount)
        {
            return episodesCount >= 0;
        }
    }
}