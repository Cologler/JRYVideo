using System;

namespace JryVideo.SearchEngine
{
    public interface ISearchEngine
    {
        void SearchText(string text);

        string Name { get; }

        int Order { get; }
    }
}