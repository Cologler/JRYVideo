using System;
using System.Collections.Generic;
using System.Linq;

namespace JryVideo.Model
{
    public sealed class JryVideo : JryObject
    {
        public JryVideo()
        {
            this.Names = new List<string>();
            this.Entities = new List<JryEntity>();
            this.Tags = new List<string>();
            this.ArtistIds = new List<Guid>();
        }

        public string Type { get; set; }

        public int Year { get; set; }

        public int Index { get; set; }

        public List<Guid> ArtistIds { get; set; }

        public List<string> Names { get; set; }

        public List<JryEntity> Entities { get; set; }

        public string DoubanId { get; set; }

        public string ImdbId { get; set; }

        public int EpisodesCount { get; set; }

        public List<string> Tags { get; set; }

        public string CoverId { get; set; }

        public override IEnumerable<string> CheckError()
        {
            foreach (var error in base.CheckError())
            {
                yield return error;
            }

            if (this.Names == null || this.Tags == null || this.Entities == null || this.ArtistIds == null)
            {
                yield return "error InitializeInstance()";
            }

            if (String.IsNullOrWhiteSpace(this.Type))
            {
                yield return "error Type";
            }

            if (this.Year < 1900 || this.Year > 2100)
            {
                yield return "error Year";
            }

            if (this.Index < 1 || this.Index > 100)
            {
                yield return "error Index";
            }

            if (this.EpisodesCount < 1)
            {
                yield return "error EpisodesCount";
            }
        }
    }
}