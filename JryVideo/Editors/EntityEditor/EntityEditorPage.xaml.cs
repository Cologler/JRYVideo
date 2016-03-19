using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Editors.EntityEditor
{
    /// <summary>
    /// EntityCreatorPage.xaml 的交互逻辑
    /// </summary>
    public partial class EntityEditorPage : Page
    {
        public EntityEditorViewModel ViewModel { get; private set; } = new EntityEditorViewModel();

        public EntityEditorPage()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.ViewModel.ParseFiles(files);
            }
        }
    }
}
