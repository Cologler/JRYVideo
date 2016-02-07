using JryVideo.Common;
using JryVideo.Model;

namespace JryVideo.Editors.ArtistEditor
{
    public class ArtistEditorViewModel : EditorItemViewModel<JryArtist>
    {
        public NameEditableViewModel<JryArtist> Names { get; }
            = new NameEditableViewModel<JryArtist>(false);

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