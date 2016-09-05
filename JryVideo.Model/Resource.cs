using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using JryVideo.Model.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed class Resource : RootObject, IQueryBy<Resource.QueryParameter>
    {
        public Resource()
        {
            this.Fansubs = new List<string>();
            this.SubTitleLanguages = new List<string>();
            this.Tags = new List<string>();
            this.TrackLanguages = new List<string>();
        }

        /// <summary>
        /// for current, video id only in same series.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public List<string> VideoIds { get; set; } = new List<string>();

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public FileNameFormat Format { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<string> Tags { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<string> Fansubs { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<string> SubTitleLanguages { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public List<string> TrackLanguages { get; set; }

        /// <summary>
        /// can not empty.
        /// </summary>
        [NotNull]
        public string Resolution { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string Quality { get; set; }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public string AudioSource { get; set; }

        /// <summary>
        /// can not empty.
        /// </summary>
        [NotNull]
        public string Extension { get; set; }

        public override void CheckError()
        {
            base.CheckError();
            DataCheck.NotNull(this.VideoIds);
            DataCheck.False(IsResolutionInvalid(this.Resolution));
            DataCheck.False(IsExtensionInvalid(this.Extension));
        }

        public static bool IsResolutionInvalid(string resolution) => resolution.IsNullOrWhiteSpace();

        public static bool IsExtensionInvalid(string extension) => extension.IsNullOrWhiteSpace();

        public struct QueryParameter
        {
            public string VideoId { get; set; }

            public JryFlagType FlagType { get; set; }

            public string FlagValue { get; set; }
        }

        public class ContentComparer : EqualityComparer<Resource>
        {
            private static bool ListEquals(List<string> x, List<string> y)
            {
                if (x == null) return y == null;
                if (y == null) return false;
                return x.SequenceEqual(y);
            }

            public override bool Equals(Resource left, Resource right)
            {
                if (ReferenceEquals(left, right)) return true;

                // single 
                if (left.AudioSource != right.AudioSource) return false;
                if (left.Quality != right.Quality) return false;
                if (left.Extension != right.Extension) return false;
                if (left.Resolution != right.Resolution) return false;

                // mulit
                if (!ListEquals(left.Fansubs, right.Fansubs)) return false;
                if (!ListEquals(left.SubTitleLanguages, right.SubTitleLanguages)) return false;
                if (!ListEquals(left.TrackLanguages, right.TrackLanguages)) return false;
                if (!ListEquals(left.Tags, right.Tags)) return false;

                return true;
            }

            public override int GetHashCode(Resource obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}