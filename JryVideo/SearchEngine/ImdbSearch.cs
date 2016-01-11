namespace JryVideo.SearchEngine
{
    public sealed class ImdbSearch : WebsiteSearch
    {
        public override string Name => nameof(ImdbSearch).Replace("Search", "").ToLower();

        protected override string BuildUrl(string keyword)
            => $"http://www.imdb.com/find?q={keyword}&s=tt";
    }
}