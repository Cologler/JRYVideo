using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IResourceDataSource : IQueryableEntitySet<Resource, Resource.QueryParameter>
    {
    }
}