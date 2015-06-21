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

namespace JryVideo.Add.SelectSeries
{
    /// <summary>
    /// SelectSeriesPage.xaml 的交互逻辑
    /// </summary>
    public partial class SelectSeriesPage : Page
    {
        public SelectSeriesPage()
        {
            this.InitializeComponent();

            this.EditSeriesUserControl.ViewModel.Created += this.EditSeriesUserControl_ViewModel_Created;
        }

        public SelectSeriesViewModel ViewModel { get; private set; }

        void EditSeriesUserControl_ViewModel_Created(object sender, JrySeries e)
        {
            if (this.Dispatcher.CheckAccessOrBeginInvoke(this.EditSeriesUserControl_ViewModel_Created, sender, e))
            {
                if (this.ViewModel != null)
                {
                    var vm = new SeriesViewModel(e);
                    this.ViewModel.SeriesList.Add(vm);
                    this.ViewModel.Selected = vm;
                    this.SeriesListView.ScrollIntoView(vm);
                }
            }
        }

        /// <summary>
        /// 引发 <see cref="E:System.Windows.FrameworkElement.Initialized"/> 事件。 每当在内部将 <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> 设置为 true 时，都将调用此方法。
        /// </summary>
        /// <param name="e">包含事件数据的 <see cref="T:System.Windows.RoutedEventArgs"/>。</param>
        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.EditSeriesUserControl.SetCreate();

            this.DataContext = this.ViewModel = new SelectSeriesViewModel();

            await this.ViewModel.LoadAsync();
        }
    }
}
