using System.Diagnostics;

namespace JryVideo.SearchEngine
{
    public sealed class WebsiteSearch : ISearchEngine
    {
        private readonly string url;

        public WebsiteSearch(string name, string url, int order)
        {
            this.url = url;
            this.Name = name;
            this.Order = order;
        }

        public void SearchText(string text)
        {
            try
            {
                using (Process.Start(this.url.Replace("%s", text)))
                {

                }
            }
            catch
            {
                // ignored
            }
        }

        public string Name { get; }

        public int Order { get; }
    }
}