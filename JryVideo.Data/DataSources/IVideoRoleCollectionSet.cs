using Jasily.Data;
using JryVideo.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface IVideoRoleCollectionSet : IJasilyEntitySetProvider<VideoRoleCollection, string>
    {
        Task<IEnumerable<VideoRoleCollection>> FindAsync(VideoRoleCollection.QueryParameter parameter);
    }
}