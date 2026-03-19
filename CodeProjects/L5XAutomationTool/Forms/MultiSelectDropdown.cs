namespace L5XAutomationTool.Forms
{
    public partial class MultiSelectDropdown : Form
    {
        #region Fields
        private bool ClosedBySelect = false;
        private readonly bool AcceptEmptyList = false;
        #endregion

        public MultiSelectDropdown(List<string> names, string text, bool acceptEmptyList = false)
        {
            if (names.Count == 0)
            {
                throw new EmptyListException("Attempted to initialize dropdown gui with no elements");
            }
            InitializeComponent();

            if (DropdownElements.Columns.Count == 0)
            {
                DropdownElements.Columns.Add("", -2);
            }

            DropdownElements.Items.AddRange(names.Select(n => new ListViewItem(n)).ToArray());
            TextBox.Text = text;
            TextBox.MaximumSize = new System.Drawing.Size(int.MaxValue, 25);
            this.AcceptEmptyList = acceptEmptyList;
        }

        #region Functions
        public void ShowDialog(Form owner, out List<string> selected)
        {
            base.ShowDialog(owner);
            if (!ClosedBySelect)
            {
                throw new AbortedElementException();
            }
            if (!AcceptEmptyList && DropdownElements.SelectedItems.Count == 0)
            {
                throw new EmptyListException("Closed GUI without selecting item");
            }
            else
                selected = [.. DropdownElements.SelectedItems.Cast<ListViewItem>().Select(i => i.Text)];
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

        private void MultiSelectDropdown_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (AcceptEmptyList && !ClosedBySelect)
            {
                DropdownElements.SelectedItems.Clear();
                ClosedBySelect = true;
                Close();
            }
        }
    }
}
