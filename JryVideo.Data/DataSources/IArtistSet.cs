using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IArtistSet : IQueryableEntitySet<Artist, Artist.QueryParameter>
    {
    }
}