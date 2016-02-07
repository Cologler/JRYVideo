using Jasily.ComponentModel;
using JryVideo.Common;
using JryVideo.Model;
using static JryVideo.Common.Helper;

namespace JryVideo.Editors.ArtistEditor
{
    public class ArtistEditorViewModel : EditorItemViewModel<JryArtist>
    {
        private string theTVDBId;
        private string doubanId;
        private string imdbId;

        public NameEditableViewModel<JryArtist> Names { get; }
            = new NameEditableViewModel<JryArtist>(false);

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

        public override void ReadFromObject(JryArtist obj)
        {
            base.ReadFromObject(obj);
            this.Names.ReadFromObject(obj);
        }

        public override void WriteToObject(JryArtist obj)
        {
            base.WriteToObject(obj);
            this.Names.WriteToObject(obj);
        }
    }
}