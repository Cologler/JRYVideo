using System.Xml.Serialization;

namespace JryVideo.Core.TheTVDB
{
    [XmlRoot("Data")]
    public class SeriesArray
    {
        [XmlElement("Series")]
        public Series[] Series { get; set; }
    }
}