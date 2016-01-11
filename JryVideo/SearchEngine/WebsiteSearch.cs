using System.Diagnostics;

namespace JryVideo.SearchEngine
{
    public abstract class WebsiteSearch : ISearchEngine
    {
        public void SearchText(string text)
        {
            try
            {
                using (Process.Start(this.BuildUrl(text)))
                {

                }
            }
            catch
            {
                // ignored
            }
        }

        public abstract string Name { get; }

        protected abstract string BuildUrl(string keyword);
    }
}