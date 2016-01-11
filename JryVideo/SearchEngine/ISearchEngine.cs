namespace JryVideo.SearchEngine
{
    public interface ISearchEngine
    {
        void SearchText(string text);

        string Name { get; }
    }
}