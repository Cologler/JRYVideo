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
using System.Windows.Navigation;
using System.Windows.Shapes;
using JryVideo.Model;

namespace JryVideo.Add.SelectVideo
{
    /// <summary>
    /// SelectVideoPage.xaml 的交互逻辑
    /// </summary>
    public partial class SelectVideoPage : Page
    {
        public SelectVideoViewModel ViewModel { get; private set; }

        public SelectVideoPage()
        {
            this.InitializeComponent();
        }

        public async Task SetSourceAsync(JrySeries series)
        {
            this.DataContext = this.ViewModel = new SelectVideoViewModel(series);
            await this.ViewModel.LoadAsync();
        }
    }
}
