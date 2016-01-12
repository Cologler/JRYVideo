using System;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JryVideo.Core.TheTVDB
{
    public class Actor
    {
        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("Image")]
        public string Image { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Role")]
        public string Role { get; set; }

        [XmlElement("SortOrder")]
        public int SortOrder { get; set; }

        public async Task<byte[]> GetBannerAsync(TheTVDBClient client)
        {
            if (this.Image.IsNullOrWhiteSpace()) return null;
            return await client.GetBannerByBannerPathAsync(this.Image);
        }
    }
}