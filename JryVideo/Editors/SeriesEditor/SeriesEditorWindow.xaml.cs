using JryVideo.Common;
using JryVideo.Model;
using MahApps.Metro.Controls;
using System.Windows;
using Jasily.Diagnostics;

namespace JryVideo.Editors.SeriesEditor
{
    /// <summary>
    /// SeriesEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SeriesEditorWindow : MetroWindow
    {
        public SeriesEditorWindow()
        {
            this.InitializeComponent();
        }

        public SeriesEditorWindow(SeriesViewModel series)
            : this()
        {
            JasilyDebug.Pointer();
            this.EditSeriesUserControl.ViewModel.Updated += this.ViewModel_Updated;
            JasilyDebug.Pointer();
            this.EditSeriesUserControl.ViewModel.ModifyMode(series);
            JasilyDebug.Pointer();
        }

        void ViewModel_Updated(object sender, JrySeries e)
        {
            if (this.GetUIDispatcher().CheckAccessOrBeginInvoke(this.ViewModel_Updated, sender, e))
            {
                this.DialogResult = true;
            }
        }
    }
}
