using System.Xml.Serialization;

namespace JryVideo.Core.TheTVDB
{
    [XmlRoot("Actors")]
    public class ActorArray
    {
        [XmlElement("Actor")]
        public Actor[] Actors { get; set; }
    }
}