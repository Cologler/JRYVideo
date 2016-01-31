using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface IVideoRoleCollectionSet : IJasilyEntitySetProvider<VideoRoleCollection, string>
    {
        Task<IEnumerable<VideoRoleCollection>> FindAsync(VideoRoleCollection.QueryParameter parameter);
    }
}