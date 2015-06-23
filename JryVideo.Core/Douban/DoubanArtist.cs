using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JryVideo.Core.Douban
{
    [DataContract]
    public class DoubanArtist : DoubanEntity
    {
        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "name_en")]
        public string OriginName;

        [DataMember(Name = "aka_en")]
        public List<string> OtherOriginNames;

        [DataMember(Name = "avatars")]
        public DoubanImages Images;

        public override string GetLargeImageUrl()
        {
            return this.Images.Large.ThrowIfNullOrEmpty("Large");
        }
    }
}