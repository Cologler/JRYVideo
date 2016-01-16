namespace JryVideo.SearchEngine
{
    public interface ISearchEngine : IOrder
    {
        void SearchText(string text);

        string Name { get; }
    }
}