using JryVideo.Model;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace JryVideo.Data.DataSources
{
    public interface ICoverSet : IJasilyEntitySetProvider<JryCover, string>
    {
        Task<IEnumerable<JryCover>> QueryByDoubanIdAsync(JryCoverType coverType, string doubanId);

        Task<IEnumerable<JryCover>> QueryByImdbIdAsync(JryCoverType coverType, string imdbId);

        Task<IEnumerable<JryCover>> QueryByUriAsync(JryCoverType coverType, string uri);
    }
}