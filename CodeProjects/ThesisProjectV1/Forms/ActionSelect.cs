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
            try
            {
                Execute.ImportElement();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error creating element. Aborting action.");
            }
        }
        private void GenerateSelect_Click(object sender, EventArgs e)
        {
            try
            {
                Execute.GenerateElement();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error creating element. Aborting action.");
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ModifySelect_Click(object sender, EventArgs e)
        {
            try
            {
                Execute.ModifyElement();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error creating element. Aborting action.");
            }
        }

        private void DeleteSelect_Click(object sender, EventArgs e)
        {
            try
            {
                Execute.DeleteElement();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error creating element. Aborting action.");
            }
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
            MessageBox.Show("New Empty File Created.");
        }
    }
}
