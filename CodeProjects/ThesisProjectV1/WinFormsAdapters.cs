
// ThesisProjectV1/WinFormsAdapters/UiAdapters.cs
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ThesisProjectV1.Abstractions;
using ThesisProjectV1.Forms;

namespace ThesisProjectV1.WinFormsAdapters
{
    public sealed class MessageService : IMessageService
    {
        public void Show(string message, string title)
        {
            MessageBox.Show(message, title ?? "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title ?? "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public bool Confirm(string message, string title)
        {
            return MessageBox.Show(message, title ?? "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                   == DialogResult.Yes;
        }
    }

    public sealed class OpenFileService : IOpenFileService
    {
        public bool TryOpen(string filter, out string filePath, string initialDirectory)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = filter;
                ofd.FilterIndex = 0;
                ofd.RestoreDirectory = true;
                if (!string.IsNullOrWhiteSpace(initialDirectory))
                {
                    ofd.InitialDirectory = initialDirectory;
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    filePath = ofd.FileName;
                    return true;
                }
            }
            filePath = string.Empty;
            return false;
        }
    }

    public sealed class SaveFileService : ISaveFileService
    {
        public bool TrySave(string suggestedName, string filter, string defaultExt, out string filePath)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = filter;
                sfd.FileName = suggestedName;
                sfd.DefaultExt = defaultExt;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    filePath = sfd.FileName;
                    return true;
                }
            }
            filePath = string.Empty;
            return false;
        }
    }

    public sealed class UserPromptService : IUserPromptService
    {
        public string SelectOne(string prompt, IList<string> options, string title)
        {
            var dlg = new DropdownGui(options.ToList(), prompt);
            string selected;
            dlg.ShowDialog(out selected);
            return selected;
        }

        public IList<string> SelectMany(string prompt, IList<string> options, bool allowNone, string title)
        {
            var dlg = new MultiSelectDropdown(options.ToList(), prompt, allowNone);
            List<string> selected;
            dlg.ShowDialog(out selected);
            return selected;
        }

        public string Prompt(string prompt, string defaultValue, string regex, string title)
        {
            var dlg = new TextInput(prompt, defaultValue, regex);
            string value;
            dlg.ShowDialog(out value);
            return value;
        }
    }
}