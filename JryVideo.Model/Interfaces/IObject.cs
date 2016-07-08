using Jasily.Data;

namespace JryVideo.Model.Interfaces
{
    public interface IObject : IJasilyEntity<string>
    {
        int Version { get; set; }
    }
}