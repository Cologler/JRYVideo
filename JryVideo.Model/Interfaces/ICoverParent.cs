namespace JryVideo.Model.Interfaces
{
    public interface ICoverParent : IObject
    {
        string CoverId { get; set; }

        CoverType CoverType { get; }
    }
}