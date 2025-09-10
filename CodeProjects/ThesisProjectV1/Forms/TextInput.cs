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

        public TextInput(string header, string defaultText = "", string reg = @"\w")
        {
            InitializeComponent();
            UserInstructionTextBox.Text = header;
            InputValueName.Text = defaultText;
            regex = reg;
        }

        public void ShowDialog(out string textValue)
        {
            base.ShowDialog();
            textValue = InputValueName.Text;
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(InputValueName.Text, regex))
                Close();
            else
                UserInstructionTextBox.Text = "Invalid Input";
        }

        private void InputValueName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && Regex.IsMatch(InputValueName.Text, regex))
                Close();
            else if (e.KeyCode == Keys.Enter)
                UserInstructionTextBox.Text = "Invalid Input";
        }
    }
}