using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThesisProjectV1.Forms
{
    public partial class TextInput : Form
    {
        public TextInput()
        {
            InitializeComponent();
        }

        public TextInput(string header, string defaultText)
        {
            InitializeComponent();
            this.UserInstructionTextBox.Text = header;
            this.InputValueName.Text = defaultText;
        }

        public void ShowDialog(out string textValue)
        {
            base.ShowDialog();
            textValue = this.InputValueName.Text;
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            if (this.InputValueName.Text.Length > 0)
                this.Close();
        }
    }
}
