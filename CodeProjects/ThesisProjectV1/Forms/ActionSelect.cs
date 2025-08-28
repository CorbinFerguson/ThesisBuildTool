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
            Close();
        }

        private void ModifySelect_Click(object sender, EventArgs e)
        {
            Execute.ModifyElement();
        }

        private void DeleteSelect_Click(object sender, EventArgs e)
        {
            Execute.DeleteElement();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            Execute.SaveFile();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            Execute.LoadFile();
        }

        private void NewFile_Click(object sender, EventArgs e)
        {
            Execute.NewFile();
        }
    }
}
