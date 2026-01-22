using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ThesisProjectV1.Forms
{
    public partial class MultiSelectDropdown : Form
    {
        #region Fields
        private bool ClosedBySelect = false;
        #endregion

        public MultiSelectDropdown(List<string> names, string text)
        {
            if (names.Count() == 0)
            {
                throw new EmptyListException("Attempted to initialize dropdown gui with no elements");
            }
            InitializeComponent();
            DropdownElements.Items.AddRange(names.ToArray());
            TextBox.Text = text;
            TextBox.MaximumSize = new System.Drawing.Size(int.MaxValue, 25);
        }

        #region Functions
        public void ShowDialog(out List<string> selected)
        {
            base.ShowDialog();
            if (!ClosedBySelect)
            {
                throw new AbortedElementException();
            }
            selected = DropdownElements.SelectedItems?.Cast<string>().ToList() ?? throw new EmptyListException("Closed GUI without selecting item");
        }

        private void ExitSelect_Click(object sender, EventArgs e)
        {
            if (DropdownElements.SelectedItems.Count > 0)
            {
                ClosedBySelect = true;
                Close();
            }
        }

        private void MultiSelectDropdown_Resize(object sender, EventArgs e)
        {
            // Calculate available height below the ComboBox
            int availableHeight = ClientSize.Height - (DropdownElements.Location.Y + DropdownElements.Height);

            // Set MaxDropDownHeight, ensuring it's not negative
            DropdownElements.Height = Math.Max(0, availableHeight);
        }
        #endregion
    }
}
