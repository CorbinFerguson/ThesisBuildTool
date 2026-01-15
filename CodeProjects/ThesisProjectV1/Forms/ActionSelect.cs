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

        private void ImportSelect_Click(object sender, EventArgs e)
        {
            try
            {
                Execute.ImportElement();
            }
            catch (AbortedElementException)
            {
                // do nothing
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error importing element. Aborting action.");
            }
        }
        private void GenerateSelect_Click(object sender, EventArgs e)
        {
            try
            {
                Execute.GenerateElement();
            }
            catch (AbortedElementException)
            {
                // do nothing
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error creating element. Aborting action.");
            }
        }

        private void ModifySelect_Click(object sender, EventArgs e)
        {
            try
            {
                Execute.ModifyElement();
            }
            catch (AbortedElementException)
            {
                // do nothing
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error modifying element. Aborting action.");
            }
        }

        private void DeleteSelect_Click(object sender, EventArgs e)
        {
            try
            {
                Execute.DeleteElement();
            }
            catch (AbortedElementException)
            {
                // do nothing
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error deleting element. Aborting action.");
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            Execute.SaveFile();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            Execute.LoadFile();
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            Execute.NewFile();
        }

        private void ValidateButton_Click(object sender, EventArgs e)
        {
            Execute.ValidateFile();
        }
    }
}
