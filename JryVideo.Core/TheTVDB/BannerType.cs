using System.Xml.Serialization;

namespace JryVideo.Core.TheTVDB
{
    public enum BannerType
    {
        [XmlEnum("fanart")]
        Fanart,

        [XmlEnum("poster")]
        Poster,

        [XmlEnum("season")]
        Season,

        [XmlEnum("series")]
        Series,
    }
}