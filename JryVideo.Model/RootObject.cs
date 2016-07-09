using JryVideo.Model.Interfaces;

namespace JryVideo.Model
{
    public abstract class RootObject : JryObject, IObject
    {
        public int Version { get; set; }
    }
}