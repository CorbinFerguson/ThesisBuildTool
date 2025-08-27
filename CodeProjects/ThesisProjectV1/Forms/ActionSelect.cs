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
            this.Close();
        }
        private void GenerateSelect_Click(object sender, EventArgs e)
        {
            Execute.GenerateElement();
            this.Close();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
