using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Controls.NumberEditerControl
{
    /// <summary>
    /// NumberEditerControl.xaml 的交互逻辑
    /// </summary>
    public partial class NumberEditerControl : UserControl
    {
        public static readonly DependencyProperty NumberDependencyProperty
            = DependencyProperty.Register("Number", typeof(int), typeof(NumberEditerControl), new FrameworkPropertyMetadata(NumberDependencyPropertyCallback));

        private static void NumberDependencyPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumberEditerControl)d).ValueTextBlock.Text = e.NewValue.ToString();
        }

        public NumberEditerControl()
        {
            this.InitializeComponent();
            this.Number = default(int);
        }

        public int Number
        {
            get { return (int)this.GetValue(NumberDependencyProperty); }
            set { this.SetValue(NumberDependencyProperty, value); }
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e) => this.Number++;

        private void SubButton_OnClick(object sender, RoutedEventArgs e) => this.Number--;
    }
}
