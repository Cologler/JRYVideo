using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JryVideo.Viewer.FilesViewer;

namespace JryVideo.SearchEngine
{
    public sealed class EverythingSearch : ISearchEngine
    {
        public string Name => nameof(EverythingSearch).Replace("Search", "").ToLower();

        public int Order => 1;

        public async void SearchText(string text)
        {
            var items = await this.SearchByEveryThingAsync(text);
            var dlg = new FilesViewerWindow(new FilesViewerViewModel());
            dlg.ViewModel.FilesView.Collection.AddRange(items.Select(z => new FileItemViewModel(z)));
            dlg.ShowDialog();
        }

        private async Task<IEnumerable<string>> SearchByEveryThingAsync(string text)
        {
            if (text == null) return Enumerable.Empty<string>();

            return await Task.Run(() =>
            {
                var search = new Jasily.EverythingSDK.EverythingSearch();
                search.Parameters.IsMatchPath = false;
                search.Parameters.IsMatchWholeWord = false;
                search.Parameters.IsMatchCase = false;
                search.Parameters.IsRegex = false;
                return search.SearchAll(text, 100).SelectMany(z => z).ToArray();
            });
        }
    }
}