using JryVideo.Model;
using System;
using System.Windows;
using System.Windows.Controls;

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

        public EditArtistViewModel ViewModel { get; } = new EditArtistViewModel();

        public EditArtistUserControl()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        public void SetCreate(JryArtist artist)
        {
            this.ViewModel.CreateMode();
        }

        public void SetModify(JryArtist artist)
        {
            this.ViewModel.ModifyMode(artist);
        }

        private void CommitButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.CommitClicked.Fire(sender, e);
        }
    }
}
