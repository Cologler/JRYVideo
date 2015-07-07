using System;
using System.ComponentModel;
using MahApps.Metro.Controls;

namespace JryVideo.Selectors.ArtistSelector
{
    /// <summary>
    /// ArtistSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ArtistSelectorWindow : MetroWindow
    {
        public ArtistSelectorPage ArtistSelectorPage { get; private set; }

        public ArtistSelectorWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Window.SourceInitialized"/> 事件。
        /// </summary>
        /// <param name="e">一个 <see cref="T:System.EventArgs"/>，其中包含事件数据。</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.ArtistSelectorPage = new ArtistSelectorPage();
                this.ContentFrame.Navigate(this.ArtistSelectorPage);
            }
        }
    }
}
