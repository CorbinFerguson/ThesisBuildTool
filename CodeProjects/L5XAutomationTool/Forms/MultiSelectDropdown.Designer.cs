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
            this.ExitSelect = new System.Windows.Forms.Button();
            this.TextBox = new System.Windows.Forms.TextBox();
            this.DropdownElements = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExitSelect
            // 
            this.ExitSelect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ExitSelect.Location = new System.Drawing.Point(334, 23);
            this.ExitSelect.Name = "ExitSelect";
            this.ExitSelect.Size = new System.Drawing.Size(102, 27);
            this.ExitSelect.TabIndex = 5;
            this.ExitSelect.Text = "Confirm";
            this.ExitSelect.UseVisualStyleBackColor = true;
            this.ExitSelect.Click += new System.EventHandler(this.ExitSelect_Click);
            // 
            // TextBox
            // 
            this.TextBox.AccessibleName = "Title";
            this.TextBox.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
            this.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.TextBox, 2);
            this.TextBox.Location = new System.Drawing.Point(3, 3);
            this.TextBox.MaximumSize = new System.Drawing.Size(4, 25);
            this.TextBox.Name = "TextBox";
            this.TextBox.ReadOnly = true;
            this.TextBox.Size = new System.Drawing.Size(4, 25);
            this.TextBox.TabIndex = 4;
            // 
            // DropdownElements
            // 
            this.DropdownElements.AccessibleName = "Elements";
            this.DropdownElements.AccessibleRole = System.Windows.Forms.AccessibleRole.List;
            this.DropdownElements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DropdownElements.FormattingEnabled = true;
            this.DropdownElements.HorizontalScrollbar = true;
            this.DropdownElements.Location = new System.Drawing.Point(3, 23);
            this.DropdownElements.Name = "DropdownElements";
            this.DropdownElements.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.DropdownElements.Size = new System.Drawing.Size(325, 238);
            this.DropdownElements.Sorted = true;
            this.DropdownElements.TabIndex = 3;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75.5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.5F));
            this.tableLayoutPanel1.Controls.Add(this.ExitSelect, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.TextBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.DropdownElements, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(439, 263);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // MultiSelectDropdown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(439, 263);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(223, 128);
            this.Name = "MultiSelectDropdown";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MultiSelectDropdown";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MultiSelectDropdown_FormClosed);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ExitSelect;
        private System.Windows.Forms.TextBox TextBox;
        private System.Windows.Forms.ListBox DropdownElements;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}