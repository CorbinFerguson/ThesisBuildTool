using System;
using System.Windows.Forms;
using System.Xml.Serialization.Configuration;

namespace ThesisProjectV1.Forms
{
    public partial class ActionSelect : Form
    {
        public ActionSelect()
        {
            InitializeComponent();
        }

        public enum Actions { Import, Generate, Error }

        private void ImportSelect_Click(object sender, EventArgs e)
        {
            Execute.ImportElement();
        }
        private void GenerateSelect_Click(object sender, EventArgs e)
        {
            Execute.GenerateElement();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
