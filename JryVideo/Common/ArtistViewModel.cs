using JryVideo.Common.Windows;
using JryVideo.Editors.ArtistEditor;
using JryVideo.Model;
using System.Threading.Tasks;
using System.Windows;

namespace JryVideo.Common
{
    public sealed class ArtistViewModel : HasCoverViewModel<JryArtist>
    {
        public ArtistViewModel(JryArtist source)
            : base(source)
        {
            this.NameView = new NameableViewModel<JryArtist>(source);
        }

        public string Name => string.Join(" / ", this.Source.Names);

        public NameableViewModel<JryArtist> NameView { get; }

        /// <summary>
        /// the method will call PropertyChanged for each property which has [NotifyPropertyChanged]
        /// </summary>
        public override void RefreshProperties()
        {
            base.RefreshProperties();
            this.NameView.RefreshProperties();
        }

        public async Task EditAsync(Window parent)
        {
            var page = new ArtistEditorPage();
            page.ViewModel.ReadFromObject(this.Source);
            if (page.ShowDialog(parent) == true)
            {
                page.ViewModel.WriteToObject(this.Source);
                this.RefreshProperties();
                await this.GetManagers().ArtistManager.UpdateAsync(this.Source);
            }
        }
    }
}