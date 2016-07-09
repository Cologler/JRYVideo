using Jasily.Data;

namespace JryVideo.Model.Interfaces
{
    public interface ICoverParent : IJasilyEntity<string>
    {
        string CoverId { get; set; }

        JryCoverType CoverType { get; }
    }
}