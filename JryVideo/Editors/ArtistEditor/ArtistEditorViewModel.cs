using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Core.Douban;
using JryVideo.Model;
using System.Linq;
using System.Threading.Tasks;
using static JryVideo.Common.Helper;

namespace JryVideo.Editors.ArtistEditor
{
    public class ArtistEditorViewModel : EditorItemViewModel<Artist>
    {
        private string theTVDBId;
        private string doubanId;
        private string imdbId;

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

        public override void ReadFromObject(Artist obj)
        {
            base.ReadFromObject(obj);
            this.Names.ReadFromObject(obj);
        }

        public override void WriteToObject(Artist obj)
        {
            base.WriteToObject(obj);
            this.Names.WriteToObject(obj);
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