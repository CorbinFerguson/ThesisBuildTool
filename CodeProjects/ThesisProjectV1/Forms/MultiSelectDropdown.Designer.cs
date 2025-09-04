namespace ThesisProjectV1.Forms
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
            this.ExitSelect.Location = new System.Drawing.Point(418, 32);
            this.ExitSelect.Margin = new System.Windows.Forms.Padding(4);
            this.ExitSelect.Name = "ExitSelect";
            this.ExitSelect.Size = new System.Drawing.Size(127, 34);
            this.ExitSelect.TabIndex = 5;
            this.ExitSelect.Text = "Confirm";
            this.ExitSelect.UseVisualStyleBackColor = true;
            this.ExitSelect.Click += new System.EventHandler(this.ExitSelect_Click);
            // 
            // TextBox
            // 
            this.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.TextBox, 2);
            this.TextBox.Location = new System.Drawing.Point(4, 4);
            this.TextBox.Margin = new System.Windows.Forms.Padding(4);
            this.TextBox.MaximumSize = new System.Drawing.Size(4, 25);
            this.TextBox.Name = "TextBox";
            this.TextBox.ReadOnly = true;
            this.TextBox.Size = new System.Drawing.Size(4, 22);
            this.TextBox.TabIndex = 4;
            // 
            // DropdownElements
            // 
            this.DropdownElements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DropdownElements.FormattingEnabled = true;
            this.DropdownElements.HorizontalScrollbar = true;
            this.DropdownElements.ItemHeight = 16;
            this.DropdownElements.Location = new System.Drawing.Point(4, 32);
            this.DropdownElements.Margin = new System.Windows.Forms.Padding(4);
            this.DropdownElements.Name = "DropdownElements";
            this.DropdownElements.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.DropdownElements.Size = new System.Drawing.Size(406, 293);
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
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.805032F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.19497F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(549, 329);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // MultiSelectDropdown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(549, 329);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(275, 150);
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