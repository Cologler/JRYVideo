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
using JryVideo.Common.Dialogs;
using MahApps.Metro.Controls;

namespace JryVideo.Editors.PasswordEditor
{
    /// <summary>
    /// PasswordEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PasswordEditorWindow : MetroWindow
    {
        public PasswordEditorWindow()
        {
            this.InitializeComponent();
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.Password1PasswordBox.Password.IsNullOrWhiteSpace())
            {
                this.ShowJryVideoMessage("error", "must input a password.");
            }
            else if (this.Password1PasswordBox.Password != this.Password2PasswordBox.Password)
            {
                this.ShowJryVideoMessage("error", "two password was not match.");
            }
            else
            {
                this.PasswordResult = this.Password1PasswordBox.Password;
                this.DialogResult = true;
            }
        }

        internal string PasswordResult { get; private set; }
    }
}
