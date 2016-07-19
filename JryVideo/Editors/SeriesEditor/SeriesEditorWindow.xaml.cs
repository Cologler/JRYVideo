using System.Windows;
using Jasily.Diagnostics;
using JryVideo.Common;
using JryVideo.Model;
using MahApps.Metro.Controls;

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

        public SeriesEditorWindow Initialize(SeriesViewModel series)
        {
            JasilyDebug.Pointer();
            this.EditSeriesUserControl.ViewModel.Updated += this.ViewModel_Updated;
            JasilyDebug.Pointer();
            this.EditSeriesUserControl.ViewModel.ModifyMode(series);
            JasilyDebug.Pointer();
            return this;
        }

        void ViewModel_Updated(object sender, Series e)
        {
            if (this.GetUIDispatcher().CheckAccessOrBeginInvoke(this.ViewModel_Updated, sender, e))
            {
                this.DialogResult = true;
            }
        }
    }
}
