using System.Xml.Serialization;

namespace JryVideo.Core.TheTVDB
{
    public class Mirror
    {
        [XmlElement("id")]
        public int Id { get; set; }

        [XmlElement("mirrorpath")]
        public string MirrorPath { get; set; }

        [XmlElement("typemask")]
        public int TypeMask { get; set; }

        public MirrorType Type => (MirrorType)this.TypeMask;
    }
}