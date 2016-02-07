using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Common.Windows
{
    public static class JryVideoWindowExtension
    {
        public static JryVideoWindow GetWindow(this Page page)
        {
            var w = new JryVideoWindow();
            w.TitleTextBlock.Text = page.Title;
            w.ContentFrame.Width = page.Width;
            w.ContentFrame.Height = page.Height;
            w.ContentFrame.Navigate(page);
            return w;
        }

        public static bool? ShowDialog(this Page page, Window owner)
        {
            var w = page.GetWindow();
            w.Owner = owner;
            return w.ShowDialog();
        }

        public static void SetDialogResult(this Page page, bool? dialogResult)
        {
            var w = page.TryFindParent<JryVideoWindow>();
            if (w != null)
            {
                w.DialogResult = dialogResult;
            }
        }
    }
}