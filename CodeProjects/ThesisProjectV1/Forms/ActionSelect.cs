using System;
using System.Windows.Forms;

namespace L5XAutomationTool.Forms
{
    public partial class ActionSelect : Form
    {
        private readonly Execute _app;

        public ActionSelect(Execute app)
        {
            _app = app ?? throw new ArgumentNullException("app");

            InitializeComponent();
        }

        private void ImportSelect_Click(object sender, EventArgs e)
        {
            try
            {
                _app.ImportElement();
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
                _app.CreateElement();
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
                _app.ModifyElement();
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
                _app.DeleteElement();
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
            _app.SaveFile();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            _app.LoadFile();
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            _app.NewFile();
        }

        private void ValidateButton_Click(object sender, EventArgs e)
        {
            _app.ValidateFile();
        }
    }
}
