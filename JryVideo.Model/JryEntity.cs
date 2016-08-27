using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JryVideo.Model
{
    public sealed class JryEntity : JryObject, IJasilyLoggerObject<JryEntity>
    {
        public JryEntity()
        {
            this.Fansubs = new List<string>();
            this.SubTitleLanguages = new List<string>();
            this.Tags = new List<string>();
            this.TrackLanguages = new List<string>();
        }

        [CanBeNull]
        [BsonIgnoreIfDefault]
        public JryFormat Format { get; set; }

        [NotNull]
        public List<string> Tags { get; set; }

        [NotNull]
        public List<string> Fansubs { get; set; }

        [NotNull]
        public List<string> SubTitleLanguages { get; set; }

        [NotNull]
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
        public string Extension { get; set; }

        public override void CheckError()
        {
            base.CheckError();
            DataCheck.NotNull(this.Fansubs);
            DataCheck.NotNull(this.SubTitleLanguages);
            DataCheck.NotNull(this.Tags);
            DataCheck.NotNull(this.TrackLanguages);
            DataCheck.False(IsResolutionInvalid(this.Resolution));
            DataCheck.False(IsExtensionInvalid(this.Extension));
        }

        public static bool IsResolutionInvalid(string resolution)
        {
            return resolution.IsNullOrWhiteSpace();
        }

        public static bool IsExtensionInvalid(string extension)
        {
            return extension.IsNullOrWhiteSpace();
        }
    }
}