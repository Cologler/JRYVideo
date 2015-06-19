using System;
using System.Windows;
using MahApps.Metro.Controls;

namespace JryVideo.Main
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
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

            this.MainFrame.Navigate(new MainPage());
        }
    }
}
