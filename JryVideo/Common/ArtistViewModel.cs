using JryVideo.Model;

namespace JryVideo.Common
{
    public class ArtistViewModel : HasCoverViewModel<JryArtist>
    {
        public ArtistViewModel(JryArtist source)
            : base(source)
        {
            this.NameView = new NameableViewModel<JryArtist>(source);
        }

        public string Name => string.Join(" / ", this.Source.Names);

        public NameableViewModel<JryArtist> NameView { get; }
    }
}