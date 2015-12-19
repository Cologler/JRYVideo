using MahApps.Metro.Controls;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace JryVideo.Viewer.FilesViewer
{
    /// <summary>
    /// FilesViewerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FilesViewerWindow : MetroWindow
    {
        public FilesViewerViewModel ViewModel { get; }

        public FilesViewerWindow()
        {
            this.InitializeComponent();
        }

        public FilesViewerWindow(FilesViewerViewModel viewModel)
            : this()
        {
            this.DataContext = this.ViewModel = viewModel;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var file = ((FrameworkElement)sender).DataContext as FileItemViewModel;
            if (file == null) return;
            if (e.ClickCount == 2)
            {
                if (file.IsExists)
                {
                    try
                    {
                        using (Process.Start(file.Source)) { }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private void OpenContainerFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var file = ((FrameworkElement)sender).DataContext as FileItemViewModel;
            if (file == null) return;
            if (file.IsExists)
            {
                try
                {
                    using (Process.Start(Path.GetDirectoryName(file.Source))) { }
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}
