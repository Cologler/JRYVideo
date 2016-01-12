using System.Xml.Serialization;

namespace JryVideo.Core.TheTVDB
{
    [XmlRoot("Banners")]
    public class BannerArray
    {
        [XmlElement("Banner")]
        public Banner[] Banners { get; set; }
    }
}