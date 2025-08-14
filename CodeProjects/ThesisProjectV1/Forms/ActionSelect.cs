using System;
using System.Windows.Forms;

namespace ThesisProjectV1.Forms
{
    public partial class ActionSelect : Form
    {
        public ActionSelect()
        {
            InitializeComponent();
        }

        public enum Actions { Import, Generate, Error }

        public void ShowDialog(out Actions buttonPress)
        {
            base.ShowDialog();
            if (this.DialogResult == DialogResult.OK)
                buttonPress = Actions.Import;
            else if (this.DialogResult == DialogResult.Yes)
                buttonPress = Actions.Generate;
            else
                buttonPress = Actions.Error;
        }

        private void ImportSelect_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void GenerateSelect_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }
    }
}
