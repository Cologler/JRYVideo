using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public JryFormat Format { get; set; }

        public List<string> Tags { get; set; }

        public List<string> Fansubs { get; set; }

        public List<string> SubTitleLanguages { get; set; }

        public List<string> TrackLanguages { get; set; }

        /// <summary>
        /// can not empty.
        /// </summary>
        public string Resolution { get; set; }
        
        public string FilmSource { get; set; }
        
        public string AudioSource { get; set; }

        /// <summary>
        /// can not empty.
        /// </summary>
        public string Extension { get; set; }

        protected override bool InnerTestHasError()
        {
            if (base.InnerTestHasError()) return true;

            if (this.Fansubs == null ||
                this.SubTitleLanguages == null ||
                this.Tags == null ||
                this.TrackLanguages == null)
            {
                throw new ArgumentException();
            }

            if (IsResolutionInvalid(this.Resolution))
            {
                this.Log(JasilyLogger.LoggerMode.Debug, "entity resolution can not be empty.");
                return true;
            }

            if (IsExtensionInvalid(this.Extension))
            {
                this.Log(JasilyLogger.LoggerMode.Debug, "entity extension can not be empty.");
                return true;
            }

            return false;
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