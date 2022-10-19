using System.Windows.Forms;
using Nmkoder.Forms;

namespace StableDiffusionGui.Ui
{
    internal static class UiUtils
    {
        public enum MessageType
        { Message, Warning, Error };

        public static DialogResult ShowMessageBox(string text, MessageType type = MessageType.Message, MessageForm.FontSize fontSize = MessageForm.FontSize.Normal)
        {
            MessageForm form = new MessageForm(text, $"{type}")
            {
                MsgFontSize = fontSize
            };
            form.ShowDialog();
            return DialogResult.OK;
        }

        public static DialogResult ShowMessageBox(string text, string title, MessageBoxButtons btns = MessageBoxButtons.OK)
        {
            MessageForm form = new MessageForm(text, title, btns);
            return form.ShowDialog();
        }
    }
}
