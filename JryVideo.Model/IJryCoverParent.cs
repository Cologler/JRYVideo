using System.Data;

namespace JryVideo.Model
{
    public interface IJryCoverParent : IJasilyEntity<string>
    {
        string CoverId { get; set; }
    }
}