using System.Text.RegularExpressions;

namespace L5XAutomationTool.Forms
{
    public partial class TextInput : Form
    {
        #region Fields
        private readonly string regex;
        private bool ClosedBySelect = false;
        #endregion

        #region Constructors
        public TextInput()
        {
            InitializeComponent();
        }

        public TextInput(string header, string defaultText = "", string reg = null, string title = null)
        {
            InitializeComponent();
            UserInstructionTextBox.Text = header;
            InputValueName.Text = defaultText;
            regex = reg ?? @"^\w+$";
            this.Name = title ?? string.Empty;
        }
        #endregion

        #region Functions
        public void ShowDialog(Form owner, out string textValue)
        {
            base.ShowDialog(owner);
            if (!ClosedBySelect)
            {
                throw new AbortedElementException();
            }
            textValue = InputValueName.Text;
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(InputValueName.Text, regex))
            {
                ClosedBySelect = true;
                Close();
            }
            else
                UserInstructionTextBox.Text = "Invalid Input";
        }

        private void InputValueName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && Regex.IsMatch(InputValueName.Text, regex))
            {
                ClosedBySelect = true;
                Close();
            }
            else if (e.KeyCode == Keys.Enter)
                UserInstructionTextBox.Text = "Invalid Input";
        }
        #endregion
    }
}