using System;
using System.Windows;
using System.Windows.Controls;
using JryVideo.Model;

namespace JryVideo.Controls.EditArtist
{
    /// <summary>
    /// EditArtistUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class EditArtistUserControl : UserControl
    {
        /// <summary>
        /// sender was a button
        /// </summary>
        public event EventHandler CommitClicked;

        public EditArtistViewModel ViewModel { get; private set; }

        public EditArtistUserControl()
        {
            this.InitializeComponent();
        }

        public void SetCreate(JryArtist artist)
        {
            this.DataContext = this.ViewModel = new EditArtistViewModel(artist);
        }

        public void SetModify(JryArtist artist)
        {
            this.DataContext = this.ViewModel = new EditArtistViewModel(artist);
        }

        private void CommitButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.CommitClicked.Fire(sender, e);
        }
    }
}
