using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ThesisProjectV1.Forms
{
    public partial class MultiSelectDropdown : Form
    {
        private readonly bool emptyReturn;

        public MultiSelectDropdown(List<string> names, string text, bool allowEmptyReturn = false)
        {
            if (names.Count() == 0)
            {
                throw new EmptyListException("Attempted to initialize dropdown gui with no elements");
            }
            InitializeComponent();
            emptyReturn = allowEmptyReturn;
            DropdownElements.Items.AddRange(names.ToArray());
            TextBox.Text = text;
            TextBox.MaximumSize = new System.Drawing.Size(int.MaxValue, 25);
        }

        public void ShowDialog(out List<string> selected)
        {
            base.ShowDialog();
            selected = DropdownElements.SelectedItems.Cast<string>().ToList();
        }

        private void ExitSelect_Click(object sender, EventArgs e)
        {
            if (DropdownElements.SelectedItems.Count > 0 || emptyReturn)
                Close();
        }

        private void MultiSelectDropdown_Resize(object sender, EventArgs e)
        {
            // Calculate available height below the ComboBox
            int availableHeight = ClientSize.Height - (DropdownElements.Location.Y + DropdownElements.Height);

            // Set MaxDropDownHeight, ensuring it's not negative
            DropdownElements.Height = Math.Max(0, availableHeight);
        }
    }
}
