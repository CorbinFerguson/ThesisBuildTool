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
            app.SetOwner(this);
            InitializeComponent();
        }

        private void ImportSelect_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
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
            this.Enabled = true;
        }
        private void GenerateSelect_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
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
            this.Enabled = true;
        }

        private void ModifySelect_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
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
            this.Enabled = true;
        }

        private void DeleteSelect_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
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
            this.Enabled = true;
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            _app.SaveFile();
            this.Enabled = true;
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            _app.LoadFile();
            this.Enabled = true;
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            _app.NewFile();
            this.Enabled = true;
        }

        private void ValidateButton_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            _app.ValidateFile();
            this.Enabled = true;
        }
    }
}
