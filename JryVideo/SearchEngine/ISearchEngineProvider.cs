using System.Collections.Generic;

namespace JryVideo.SearchEngine
{
    public interface ISearchEngineProvider : IOrder
    {
        IEnumerable<ISearchEngine> Create();
    }
}