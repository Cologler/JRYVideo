using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;

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

            public static bool CanBeYear(string text)
            {
                Debug.Assert(text.Length > 0);
                return text.All(char.IsDigit) && text.Length < 5;
            }

            public static int GetYear(string text) => int.Parse(text);

            public static bool CanBeStar(string text)
            {
                Debug.Assert(text.Length > 0);

                var array = GetStar(text);
                Debug.Assert(array != null);
                return array.Length > 0;
            }

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

            EntityId,

            DoubanId,

            Tag,

            VideoType,

            VideoYear,

            ImdbId,

            Star
        }
    }
}