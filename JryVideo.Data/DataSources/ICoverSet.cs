using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface ICoverSet : IEntitySet<JryCover>
    {
        Task<IEnumerable<JryCover>> FindAsync(JryCover.QueryParameter parameter);
    }
}