using System.Collections.Generic;
using System.Threading.Tasks;
using JryVideo.Model;

namespace JryVideo.Data.DataSources
{
    public interface ICoverDataSourceProvider : IDataSourceProvider<JryCover>
    {
        Task<IEnumerable<JryCover>> QueryByDoubanIdAsync(JryCoverType coverType, string doubanId);

        Task<IEnumerable<JryCover>> QueryByUriAsync(JryCoverType coverType, string uri);
    }
}