using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ThesisProjectV1.Forms
{
    public partial class TextInput : Form
    {
        private readonly string regex;

        public TextInput()
        {
            InitializeComponent();
        }

        public TextInput(string header, string defaultText, string reg= @"^(?!\s*$).+")
        {
            InitializeComponent();
            this.UserInstructionTextBox.Text = header;
            this.InputValueName.Text = defaultText;
            this.regex = reg;
        }

        public void ShowDialog(out string textValue)
        {
            base.ShowDialog();
            textValue = this.InputValueName.Text;
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(this.InputValueName.Text, this.regex))
                this.Close();
        }

        private void InputValueName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && Regex.IsMatch(this.InputValueName.Text, this.regex))
                this.Close();
        }
    }
}
