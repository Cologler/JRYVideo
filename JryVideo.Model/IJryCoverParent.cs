using Jasily.Data;

namespace JryVideo.Model
{
    public interface IJryCoverParent : IJasilyEntity<string>
    {
        string CoverId { get; set; }

        JryCoverType CoverType { get; }
    }
}