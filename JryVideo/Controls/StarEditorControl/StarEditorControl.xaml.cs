using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JryVideo.Controls.StarEditorControl
{
    /// <summary>
    /// StarEditorControl.xaml 的交互逻辑
    /// </summary>
    public partial class StarEditorControl : UserControl
    {
        public static readonly DependencyProperty ValueDependencyProperty = DependencyProperty.Register("Value",
            typeof(int), typeof(StarEditorControl), new PropertyMetadata(OnValueChangedCallback));

        private static void OnValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StarEditorControl)d).SetValue((int)e.NewValue);
        }

        public StarEditorControl()
        {
            this.InitializeComponent();
            this.Value = -1;
            this.Value = 0;
        }

        private void SetValue(int value)
        {
            foreach (var i in Enumerable.Range(1, 5))
            {
                var btn = this.GetRadioButton(i);
                if (i == value)
                {
                    if (btn.IsChecked != true)
                    {
                        btn.IsChecked = true;
                    }
                }
                else
                {
                    if (btn.IsChecked != false)
                    {
                        btn.IsChecked = false;
                    }
                }
            }

            this.TipTextBlock.Text = this.GetTipText(value);
        }

        public int Value
        {
            get { return (int)this.GetValue(ValueDependencyProperty); }
            set { this.SetValue(ValueDependencyProperty, value); }
        }

        private RadioButton GetRadioButton(int value)
        {
            switch (value)
            {
                case 1:
                    return this.Star1RadioButton;
                case 2:
                    return this.Star2RadioButton;
                case 3:
                    return this.Star3RadioButton;
                case 4:
                    return this.Star4RadioButton;
                case 5:
                    return this.Star5RadioButton;
                default:
                    return null;
            }
        }

        private string GetTipText(int value)
        {
            switch (value)
            {
                case 1:
                    return Properties.Resources.StarTip_1;
                case 2:
                    return Properties.Resources.StarTip_2;
                case 3:
                    return Properties.Resources.StarTip_3;
                case 4:
                    return Properties.Resources.StarTip_4;
                case 5:
                    return Properties.Resources.StarTip_5;
                default:
                    return Properties.Resources.StarTip_Default;
            }
        }

        private void Star1RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            this.Value = 1;
        }

        private void Star2RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            this.Value = 2;
        }

        private void Star3RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            this.Value = 3;
        }

        private void Star4RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            this.Value = 4;
        }

        private void Star5RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            this.Value = 5;
        }
    }
}
