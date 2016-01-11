namespace JryVideo.SearchEngine
{
    public sealed class WikiSearch : WebsiteSearch
    {
        public override string Name => nameof(WikiSearch).Replace("Search", "").ToLower();

        protected override string BuildUrl(string keyword)
            => $"https://en.wikipedia.org/wiki/{keyword}";
    }
}