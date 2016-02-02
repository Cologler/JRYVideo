using System.Data;

namespace JryVideo.Model
{
    public interface ITheTVDBItem : IJasilyEntity<string>
    {
        string TheTVDBId { get; set; }
    }
}