using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IVideoRoleCollectionSet : IEntitySet<VideoRoleCollection>
    {
        Task<IEnumerable<VideoRoleCollection>> FindAsync(VideoRoleCollection.QueryParameter parameter);
    }
}