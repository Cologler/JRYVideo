using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using JryVideo.Model.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed class Series : RootObject, IJryChild<JryVideoInfo>, INameable, IImdbItem, ITheTVDBItem, ITagable,
        IQueryBy<Series.QueryParameter>
    {
        public Series()
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

        public void CombineFrom(Series other)
        {
            this.Names = this.Names.Concat(other.Names).Distinct().ToList();
            this.Videos = this.Videos.Concat(other.Videos).ToList();
            other.Videos = new List<JryVideoInfo>();

            this.Tags = CombineStrings(this.Tags, other.Tags);

            if (!CanCombineField(this.ImdbId, other.ImdbId)) throw new InvalidOperationException();
            this.ImdbId = this.ImdbId ?? other.ImdbId;

            if (!CanCombineField(this.TheTVDBId, other.TheTVDBId)) throw new InvalidOperationException();
            this.TheTVDBId = this.TheTVDBId ?? other.TheTVDBId;

            if (!CanCombineField(this.WorldLineId, other.WorldLineId)) throw new InvalidOperationException();
            this.WorldLineId = this.WorldLineId ?? other.WorldLineId;
        }

        public override void CheckError()
        {
            base.CheckError();
            DataCheck.NotNull(this.Names);
            DataCheck.NotNull(this.Videos);
            DataCheck.NotEmpty(this.Names);
            DataCheck.ItemNotWhiteSpace(this.Names);
            this.Videos.ForEach(z => z.CheckError());
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

            public static bool CanBeYear(string text) => text.Length == 4 && text.All(char.IsDigit);

            public static int GetYear(string text) => int.Parse(text);

            public static bool CanBeStar(string text) => GetStar(text).Length > 0;

            public static int[] GetStar(string text)
            {
                text = text.Replace(" ", "");

                switch (text.Length)
                {
                    case 1:
                        var index = StarCharRange.IndexOf(text[0]);
                        if (index >= 0) return new[] { index + 1 };
                        break;

                    case 2: // [-]?[/d][-]?
                        // "2-" => "2-5" => 2345, "-4" => "1-4" => 1234
                        if (text[0] == '-' && StarCharRange.Contains(text[1])) // -d
                        {
                            return GetStar("1" + text); // 1-d
                        }
                        if (text[1] == '-' && StarCharRange.Contains(text[0])) // d-
                        {
                            return GetStar(text + "5"); // d-5
                        }
                        break;

                    case 3: // [/d][-][/d]
                        // 2-4 => 234
                        if (text[1] == '-')
                        {
                            var l = StarCharRange.IndexOf(text[0]);
                            var r = StarCharRange.IndexOf(text[2]);
                            if (l >= 0 && r >= 0)
                            {
                                var min = Math.Min(l, r);
                                var max = Math.Max(l, r) + 1;
                                return Enumerable.Range(1, 5).Skip(min).Take(max - min).ToArray();
                            }
                        }
                        break;
                }

                return Empty<int>.Array;
            }

            private static readonly char[] StarCharRange;

            static QueryParameter()
            {
                StarCharRange = Enumerable.Range(1, 5).Select(z => z.ToString()[0]).ToArray();
            }
        }

        public enum QueryMode
        {
            Any,

            OriginText,

            SeriesId,

            VideoId,

            ResourceId,

            DoubanId,

            Tag,

            VideoType,

            VideoYear,

            ImdbId,

            Star,

            WorldLineId
        }

        [CanBeNull]
        public string WorldLineId { get; set; }
    }
}