using System.Linq;
using System.Threading.Tasks;
using Jasily.ComponentModel.Editable;
using JryVideo.Common;
using JryVideo.Core.Douban;
using JryVideo.Model;
using static JryVideo.Common.Helper;

namespace JryVideo.Editors.ArtistEditor
{
    public class ArtistEditorViewModel : EditorItemViewModel<Artist>
    {
        private string theTVDBId;
        private string doubanId;
        private string imdbId;

        [EditableField(IsSubEditableViewModel = true)]
        public NameEditableViewModel<Artist> Names { get; }
            = new NameEditableViewModel<Artist>(false);

        [EditableField]
        public string TheTVDBId
        {
            get { return this.theTVDBId; }
            set { this.SetPropertyRef(ref this.theTVDBId, value); }
        }

        [EditableField]
        public string DoubanId
        {
            get { return this.doubanId; }
            set { this.SetPropertyRef(ref this.doubanId, TryGetDoubanId(value)); }
        }

        [EditableField]
        public string ImdbId
        {
            get { return this.imdbId; }
            set { this.SetPropertyRef(ref this.imdbId, TryGetImdbId(value)); }
        }

        public async Task LoadFromDoubanAsync()
        {
            var doubanId = this.DoubanId;
            if (string.IsNullOrWhiteSpace(doubanId)) return;
            var info = await DoubanHelper.TryGetArtistInfoAsync(doubanId);
            if (info == null) return;
            var names = DoubanHelper.ParseName(info).ToArray();
            if (names.Length > 0)
            {
                for (var i = 0; i < names.Length; i++)
                {
                    var n = names[i];
                    names[i] = n
                        .Replace("， ", "，")
                        .Replace(" ，", "，")
                        .Replace("，", " ， ")
                        .Replace("(蠻各)", "")
                        .Replace("(云兆)", "");
                }
                this.Names.AddRange(names);
            }
        }
    }
}