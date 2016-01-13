namespace JryVideo.SearchEngine
{
    public sealed class TheTVDBSearch : WebsiteSearch
    {
        public override string Name => nameof(TheTVDBSearch).Replace("Search", "").ToLower();

        protected override string BuildUrl(string keyword)
            => $"https://thetvdb.com/index.php?string={keyword}&searchseriesid=&tab=listseries&function=Search&alllang=1";
    }
}