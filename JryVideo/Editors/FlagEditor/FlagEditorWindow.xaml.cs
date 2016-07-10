using System.Windows;
using JryVideo.Common;
using JryVideo.Model;

namespace JryVideo.Editors.FlagEditor
{
    /// <summary>
    /// FlagEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FlagEditorWindow
    {
        public FlagEditorWindow()
        {
            this.InitializeComponent();
        }

        public JryFlag OriginFlag { get; private set; }

        public void InitializeEditor(JryFlag flag)
        {
            this.OriginFlag = flag;

            this.EditFlagUserControl.ViewModel.FlagType = flag.Type;
            this.EditFlagUserControl.ViewModel.ModifyMode(flag);
            this.EditFlagUserControl.ViewModel.Updated += (z, x) =>
                this.GetUIDispatcher().BeginInvoke(() => this.DialogResult = true);

            this.TitleTextBlock.Text = string.Format(
                Properties.Resources.FlagEditorWindow_Title_Format,
                flag.Type.GetLocalizeString());
            this.OriginValueTextBlock.Text = flag.Value;
        }
    }
}
