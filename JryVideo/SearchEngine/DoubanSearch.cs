namespace JryVideo.SearchEngine
{
    public sealed class DoubanSearch : WebsiteSearch
    {
        public override string Name => nameof(DoubanSearch).Replace("Search", "").ToLower();

        protected override string BuildUrl(string keyword)
            => $"http://movie.douban.com/subject_search?search_text={keyword}&cat=1002";
    }
}