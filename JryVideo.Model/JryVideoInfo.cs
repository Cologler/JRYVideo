using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using JryVideo.Model.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed partial class JryVideoInfo : JryObject, ICoverParent, INameable, IImdbItem, ITagable
    {
        public JryVideoInfo()
        {
            this.Names = new List<string>();
        }

        [BsonIgnoreIfDefault]
        public int GroupIndex { get; set; }

        public string Type { get; set; }

        public int Year { get; set; }

        public int Index { get; set; }

        [BsonIgnoreIfDefault]
        public int Star { get; set; }

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

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string ImdbId { get; set; }

        /// <summary>
        /// 是否正在追剧
        /// </summary>
        [BsonIgnoreIfDefault]
        public bool IsTracking { get; set; }

        [BsonIgnoreIfDefault]
        public bool IsAllAired { get; set; }

        [BsonIgnoreIfDefault]
        public int EpisodesCount { get; set; }

        string ICoverParent.CoverId => this.Id;

        [CanBeNull]
        [ItemNotNull]
        [BsonIgnoreIfDefault]
        public List<string> Tags { get; set; }

        [BsonIgnoreIfDefault]
        public int? EpisodeOffset { get; set; }

        public ICoverParent BackgroundImageAsCoverParent() => new BackgroundCoverParent(this);

        private sealed class BackgroundCoverParent : ICoverParent
        {
            private readonly JryVideoInfo parent;

            internal BackgroundCoverParent(JryVideoInfo parent)
            {
                this.parent = parent;
            }

            /// <summary>
            /// key of Video
            /// </summary>
            public string Id
            {
                get { return this.parent.Id; }
                set { this.parent.Id = value; }
            }

            public string CoverId => this.parent.CreateBackgroundCoverId();

            /// <summary>
            /// return 'type [id]'
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{nameof(BackgroundCoverParent)} [{this.Id}]";
        }

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

        public static bool IsYearValid(int year) => year < 2100 && year > 1900;

        public static bool IsIndexValid(int index) => index > 0;

        public static bool IsEpisodesCountValid(int episodesCount) => episodesCount >= 0;

        public void CombineFrom(JryVideoInfo other)
        {
            if (this.Type != other.Type) throw new InvalidOperationException();
            if (this.Year != other.Year) throw new InvalidOperationException();
            if (this.Index != other.Index) throw new InvalidOperationException();

            this.Names = this.Names.Concat(other.Names).Distinct().ToList();

            if (!CanCombineField(this.LastVideoId, other.LastVideoId)) throw new InvalidOperationException();
            this.LastVideoId = this.LastVideoId ?? other.LastVideoId;

            if (!CanCombineField(this.NextVideoId, other.NextVideoId)) throw new InvalidOperationException();
            this.NextVideoId = this.NextVideoId ?? other.NextVideoId;

            if (!CanCombineField(this.DoubanId, other.DoubanId)) throw new InvalidOperationException();
            this.DoubanId = this.DoubanId ?? other.DoubanId;

            if (!CanCombineField(this.ImdbId, other.ImdbId)) throw new InvalidOperationException();
            this.ImdbId = this.ImdbId ?? other.ImdbId;

            this.IsTracking = this.IsTracking || other.IsTracking;
            this.IsAllAired = this.IsAllAired || other.IsAllAired;

            if (this.EpisodesCount != other.EpisodesCount) throw new InvalidOperationException();

            this.Tags = CombineStrings(this.Tags, other.Tags);
            this.EpisodeOffset = this.EpisodeOffset ?? other.EpisodeOffset;
            this.DayOfWeek = this.DayOfWeek ?? other.DayOfWeek;
            this.StartDate = this.StartDate ?? other.StartDate;
        }

        public string CreateBackgroundCoverId() => CreateBackgroundCoverId(this.Id);

        public static string CreateBackgroundCoverId(string videoId) => videoId + ":Background";

        public override void CheckError()
        {
            base.CheckError();
            DataCheck.NotNull(this.Names);
            DataCheck.NotWhiteSpace(this.Type);
            DataCheck.True(IsYearValid(this.Year));
            DataCheck.True(IsIndexValid(this.Index));
            DataCheck.True(IsEpisodesCountValid(this.EpisodesCount));

#pragma warning disable 612
            if (this.Roles != null) this.Roles = null;
#pragma warning restore 612
            if (this.Tags?.Count == 0) this.Tags = null;
        }
    }
}