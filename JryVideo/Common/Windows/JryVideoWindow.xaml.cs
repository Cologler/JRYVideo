using System;
using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Common.Windows
{
    /// <summary>
    /// JryVideoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class JryVideoWindow
    {
        private Page contentPage;

        public JryVideoWindow()
        {
            this.InitializeComponent();
        }

        public void SetContentPage(Page page)
        {
            if (this.contentPage != null) throw new InvalidOperationException();

            this.contentPage = page;
            this.TitleTextBlock.Text = page.Title;
            this.ContentFrame.Width = page.Width;
            this.ContentFrame.Height = page.Height;
            var acceptable = page as IAcceptable;
            if (acceptable != null)
            {
                this.CommandGrid.Visibility = Visibility.Visible;
            }
            this.ContentFrame.Navigate(page);
        }

        private void AcceptButton_OnClick(object sender, System.Windows.RoutedEventArgs e)
            => (this.contentPage as IAcceptable)?.Accept();
    }
}
