using System.Data;

namespace JryVideo.Model
{
    public interface IImdbItem : IJasilyEntity<string>
    {
        string ImdbId { get; set; }
    }
}