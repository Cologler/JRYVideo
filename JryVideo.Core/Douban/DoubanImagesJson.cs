using System.Runtime.Serialization;

namespace JryVideo.Core.Douban
{
    [DataContract]
    public class DoubanImages
    {
        [DataMember(Name = "small")]
        public string Small { get; set; }

        [DataMember(Name = "large")]
        public string Large { get; set; }

        [DataMember(Name = "medium")]
        public string Medium { get; set; }
    }
}