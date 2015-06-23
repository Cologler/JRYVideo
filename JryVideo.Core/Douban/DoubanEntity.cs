using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JryVideo.Core.Douban
{
    [DataContract]
    public abstract class DoubanEntity
    {
        [DataMember(Name = "aka")]
        public List<string> OtherNames;

        public abstract string GetLargeImageUrl();
    }
}