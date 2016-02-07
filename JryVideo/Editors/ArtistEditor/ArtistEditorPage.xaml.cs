using JryVideo.Common.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Editors.ArtistEditor
{
    /// <summary>
    /// ArtistEditorPage.xaml 的交互逻辑
    /// </summary>
    public partial class ArtistEditorPage : Page, IAcceptable
    {
        public ArtistEditorPage()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        public ArtistEditorViewModel ViewModel { get; }
            = new ArtistEditorViewModel();

        public async void Accept()
        {
            if (this.ViewModel.Names.Names.IsNullOrWhiteSpace())
            {
                await this.TryFindParent<MetroWindow>().ShowMessageAsync("error", "name can not be empty.");
                return;
            }
            this.SetDialogResult(true);
        }
    }
}
