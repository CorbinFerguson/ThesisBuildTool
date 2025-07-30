using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThesisProjectV1.Forms
{
    public partial class DropdownGui : Form
    {
        public DropdownGui()
        {
            InitializeComponent();
        }
        public DropdownGui(List<string> names)
        {
            InitializeComponent();
            this.dropdownelements.Items.AddRange(names.ToArray());
        }

        public void ShowDialog(out string selected)
        {
            base.ShowDialog();
            selected = this.dropdownelements.SelectedItem as string;
        }
    }
}
