namespace L5XAutomationTool.Forms
{
    public partial class DropdownGui : Form
    {
        private bool ClosedBySelect = false;
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
            Dropdownelements.Items.AddRange(names.ToArray());
            textBox.Text = text;
        }

        #region Functions
        public void ShowDialog(Form owner, out string selected)
        {
            base.ShowDialog(owner);
            if (!ClosedBySelect)
            {
                throw new AbortedElementException();
            }
            selected = Dropdownelements.SelectedItem?.ToString() ?? throw new EmptyListException("Closed GUI without selecting item");
        }

        private void ExitSelect_Click(object sender, EventArgs e)
        {
            if (Dropdownelements.SelectedItem != null)
            {
                ClosedBySelect = true;
                Close();
            }
        }

        private void Dropdownelements_DoubleClick(object sender, EventArgs e)
        {
            if (Dropdownelements.SelectedItem != null)
            {
                ClosedBySelect = true;
                Close();
            }
        }
        #endregion
    }
}
