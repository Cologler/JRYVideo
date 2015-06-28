using System;
using System.Collections.Generic;
using System.Linq;

namespace JryVideo.Model
{
    public sealed class JryVideoInfo : JryObject, IJryCoverParent
    {
        public JryVideoInfo()
        {
            this.Names = new List<string>();
            this.Tags = new List<string>();
            this.ArtistIds = new List<Guid>();
        }

        public string Type { get; set; }

        public int Year { get; set; }

        public int Index { get; set; }

        public List<Guid> ArtistIds { get; set; }

        public List<string> Names { get; set; }

        public string DoubanId { get; set; }

        public string ImdbId { get; set; }

        public int EpisodesCount { get; set; }

        public List<string> Tags { get; set; }

        public string CoverId { get; set; }

        public override IEnumerable<JryInvalidError> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.Names == null || this.Tags == null || this.ArtistIds == null)
            {
                throw new ArgumentException();
            }

            if (String.IsNullOrWhiteSpace(this.Type))
            {
                yield return JryInvalidError.VideoTypeCanNotBeEmpty;
            }

            if (!IsYearValid(this.Year))
            {
                yield return JryInvalidError.VideoYearValueInvalid;
            }

            if (!IsIndexValid(this.Index))
            {
                yield return JryInvalidError.VideoIndexLessThanOne;
            }

            if (!IsEpisodesCountValid(this.EpisodesCount))
            {
                yield return JryInvalidError.VideoEpisodesCountLessThanOne;
            }
        }

        public static bool IsYearValid(int year)
        {
            return year < 2100 && year > 1900;
        }

        public static bool IsIndexValid(int index)
        {
            return index > 0 && index < 100;
        }

        public static bool IsEpisodesCountValid(int episodesCount)
        {
            return episodesCount > 0;
        }
    }
}