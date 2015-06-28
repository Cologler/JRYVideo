using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JryVideo.Core.Douban
{
    [DataContract]
    public abstract class DoubanEntity
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "aka")]
        public List<string> OtherNames;

        public abstract string GetLargeImageUrl();
    }
}