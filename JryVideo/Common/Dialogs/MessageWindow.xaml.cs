using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace JryVideo.Common.Dialogs
{
    /// <summary>
    /// MessageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWindow : MetroWindow
    {
        public MessageWindow()
        {
            this.InitializeComponent();
        }
    }

    internal static class MessageWindowHelper
    {
        public static void ShowJryVideoMessage(this DependencyObject self, string caption, string message)
        {
            self.TryFindParent<Window>().ShowJryVideoMessage(caption, message);
        }

        public static void ShowJryVideoMessage(this Window self, string caption, string message)
        {
            var dlg = new MessageWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = self
            };
            dlg.TitleTextBlock.Text = caption;
            dlg.ContentTextBlock.Text = message;
            dlg.ShowDialog();
        }
    }
}
