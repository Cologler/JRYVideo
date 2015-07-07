using System.Enums;
using JryVideo.Common;
using JryVideo.Model;

namespace JryVideo.Controls.EditArtist
{
    public class EditArtistViewModel : ArtistViewModel
    {
        public EditArtistViewModel(JryArtist source)
            : base(source)
        {
        }

        public ObjectChangedAction Action { get; set; }
    }
}