using System;
using System.Windows;
using JryVideo.Model;

namespace JryVideo.Selectors.SeriesSelector
{
    /// <summary>
    /// SeriesSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SeriesSelectorWindow
    {
        private readonly SeriesSelectorPage seriesSelectorPage = new SeriesSelectorPage();

        public SeriesSelectorWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> is set to true internally. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.ContentFrame.Navigate(this.seriesSelectorPage);
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
            => this.DialogResult = true;

        public static JrySeries Select(Window parent, JrySeries without = null)
        {
            var window = new SeriesSelectorWindow { Owner = parent };
            if (without != null)
            {
                window.seriesSelectorPage.SelectorViewModel.Withouts.Add(without.Id);
            }
            if (window.ShowDialog() == true) return window.seriesSelectorPage.SelectorViewModel.Items.Selected;
            return null;
        }
    }
}
