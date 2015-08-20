using System;
using System.Runtime.Serialization;

namespace JryVideo.Core.Douban
{
    [DataContract]
    public class DoubanMovie : DoubanEntity
    {
        [DataMember(Name = "images")]
        public DoubanImages Images;

        [DataMember(Name = "title")]
        public string Title;

        [DataMember(Name = "original_title")]
        public string OriginalTitle;

        [DataMember(Name = "year")]
        public string Year;

        /// <summary>
        /// "movie" or "TV"
        /// </summary>
        [DataMember(Name = "subtype")]
        public string SubType;

        [DataMember(Name = "seasons_count")]
        public int? SeasonsCount;

        [DataMember(Name = "episodes_count")]
        public int? EpisodesCount;

        public override string GetLargeImageUrl()
        {
            return this.Images.Large.ThrowIfNullOrEmpty("Large");
        }
    }
}