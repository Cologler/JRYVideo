using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        public SeriesEditorWindow(JrySeries series)
            : this()
        {
            this.EditSeriesUserControl.ViewModel.Updated += this.ViewModel_Updated;
            this.EditSeriesUserControl.ViewModel.ModifyMode(series);
        }

        void ViewModel_Updated(object sender, JrySeries e)
        {
            if (this.Dispatcher.CheckAccessOrBeginInvoke(this.ViewModel_Updated, sender, e))
            {
                this.DialogResult = true;
            }
        }
    }
}
