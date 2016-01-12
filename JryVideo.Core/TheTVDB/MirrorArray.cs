using System.Xml.Serialization;

namespace JryVideo.Core.TheTVDB
{
    [XmlRoot("Mirrors")]
    public class MirrorArray
    {
        [XmlElement("Mirror")]
        public Mirror[] Mirrors { get; set; }
    }
}