using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JryVideo.Core.Douban
{
    [DataContract]
    public class DoubanMovieJson
    {
        [DataMember(Name = "images")]
        public JsonImages Images;

        [DataMember(Name = "title")]
        public string Title;

        [DataMember(Name = "original_title")]
        public string OriginalTitle;

        [DataMember(Name = "aka")]
        public List<string> Aka;

        [DataMember(Name = "year")]
        public int Year;

        /// <summary>
        /// "movie" or "TV"
        /// </summary>
        [DataMember(Name = "subtype")]
        public string SubType;

        [DataContract]
        public class JsonImages
        {
            [DataMember(Name = "small")]
            public string Small { get; set; }

            [DataMember(Name = "large")]
            public string Large { get; set; }

            [DataMember(Name = "medium")]
            public string Medium { get; set; }
        }
    }
}