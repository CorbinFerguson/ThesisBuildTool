namespace L5XAutomationTool.Forms
{
    partial class MultiSelectDropdown
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ExitSelect = new Button();
            TextBox = new TextBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            DropdownElements = new ListView();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // ExitSelect
            // 
            ExitSelect.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ExitSelect.Location = new Point(334, 23);
            ExitSelect.Name = "ExitSelect";
            ExitSelect.Size = new Size(102, 27);
            ExitSelect.TabIndex = 5;
            ExitSelect.Text = "Confirm";
            ExitSelect.UseVisualStyleBackColor = true;
            ExitSelect.Click += ExitSelect_Click;
            // 
            // TextBox
            // 
            TextBox.AccessibleName = "Title";
            TextBox.AccessibleRole = AccessibleRole.TitleBar;
            TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.SetColumnSpan(TextBox, 2);
            TextBox.Location = new Point(3, 3);
            TextBox.MaximumSize = new Size(4, 25);
            TextBox.Name = "TextBox";
            TextBox.ReadOnly = true;
            TextBox.Size = new Size(4, 23);
            TextBox.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75.5F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24.5F));
            tableLayoutPanel1.Controls.Add(ExitSelect, 1, 1);
            tableLayoutPanel1.Controls.Add(TextBox, 0, 0);
            tableLayoutPanel1.Controls.Add(DropdownElements, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Margin = new Padding(2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(439, 263);
            tableLayoutPanel1.TabIndex = 6;
            // 
            // DropdownElements
            // 
            DropdownElements.AccessibleName = "Elements";
            DropdownElements.AccessibleRole = AccessibleRole.List;
            DropdownElements.AutoArrange = false;
            DropdownElements.Dock = DockStyle.Fill;
            DropdownElements.FullRowSelect = true;
            DropdownElements.HeaderStyle = ColumnHeaderStyle.None;
            DropdownElements.LabelWrap = false;
            DropdownElements.Location = new Point(3, 23);
            DropdownElements.Name = "DropdownElements";
            DropdownElements.Size = new Size(325, 237);
            DropdownElements.TabIndex = 6;
            DropdownElements.UseCompatibleStateImageBehavior = false;
            DropdownElements.View = View.Details;
            // 
            // MultiSelectDropdown
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(439, 263);
            Controls.Add(tableLayoutPanel1);
            MinimumSize = new Size(223, 128);
            Name = "MultiSelectDropdown";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MultiSelectDropdown";
            FormClosed += MultiSelectDropdown_FormClosed;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ExitSelect;
        private System.Windows.Forms.TextBox TextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ListView DropdownElements;
    }
}