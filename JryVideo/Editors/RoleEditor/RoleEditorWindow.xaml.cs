using System.Windows;
using JryVideo.Model;

namespace JryVideo.Editors.RoleEditor
{
    /// <summary>
    /// RoleEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RoleEditorWindow
    {
        public RoleEditorWindow()
        {
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        public RoleEditorViewModel ViewModel { get; } = new RoleEditorViewModel();

        public static bool Edit(Window parent, VideoRole role)
        {
            var window = new RoleEditorWindow() { Owner = parent };
            window.ViewModel.ReadFromObject(role);
            if (window.ShowDialog() == true)
            {
                window.ViewModel.WriteToObject(role);
                return true;
            }
            return false;
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e) => this.DialogResult = true;
    }
}
