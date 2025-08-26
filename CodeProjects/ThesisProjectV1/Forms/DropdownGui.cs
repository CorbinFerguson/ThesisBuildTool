using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ThesisProjectV1.Forms
{
    public partial class DropdownGui : Form
    {
        public DropdownGui()
        {
            InitializeComponent();
        }
        public DropdownGui(List<string> names, string text)
        {
            if (names.Count() == 0)
            {
                throw new EmptyListException("Attempted to initialize dropdown gui with no elements");
            }
            InitializeComponent();
            this.Dropdownelements.Items.AddRange(names.ToArray());
            this.textBox.Text = text;
        }

        public void ShowDialog(out string selected)
        {
            base.ShowDialog();
            selected = this.Dropdownelements.SelectedItem.ToString();
        }

        private void ExitSelect_Click(object sender, EventArgs e)
        {
            if (this.Dropdownelements.SelectedItem != null)
                this.Close();
        }

        private void Dropdownelements_DoubleClick(object sender, EventArgs e)
        {
            if (this.Dropdownelements.SelectedItem != null)
                this.Close();
        }
    }
}
