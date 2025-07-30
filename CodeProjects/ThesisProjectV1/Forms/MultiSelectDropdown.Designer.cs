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
            this.SuspendLayout();
            // 
            // ExitSelect
            // 
            this.ExitSelect.Location = new System.Drawing.Point(32, 82);
            this.ExitSelect.Name = "ExitSelect";
            this.ExitSelect.Size = new System.Drawing.Size(80, 27);
            this.ExitSelect.TabIndex = 5;
            this.ExitSelect.Text = "Confirm";
            this.ExitSelect.UseVisualStyleBackColor = true;
            this.ExitSelect.Click += new System.EventHandler(this.ExitSelect_Click);
            // 
            // TextBox
            // 
            this.TextBox.Location = new System.Drawing.Point(32, 24);
            this.TextBox.Name = "TextBox";
            this.TextBox.ReadOnly = true;
            this.TextBox.Size = new System.Drawing.Size(291, 20);
            this.TextBox.TabIndex = 4;
            // 
            // DropdownElements
            // 
            this.DropdownElements.FormattingEnabled = true;
            this.DropdownElements.HorizontalScrollbar = true;
            this.DropdownElements.Location = new System.Drawing.Point(118, 82);
            this.DropdownElements.Name = "DropdownElements";
            this.DropdownElements.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.DropdownElements.Size = new System.Drawing.Size(205, 147);
            this.DropdownElements.TabIndex = 3;
            // 
            // MultiSelectDropdown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 261);
            this.Controls.Add(this.ExitSelect);
            this.Controls.Add(this.TextBox);
            this.Controls.Add(this.DropdownElements);
            this.Name = "MultiSelectDropdown";
            this.Text = "MultiSelectDropdown";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ExitSelect;
        private System.Windows.Forms.TextBox TextBox;
        private System.Windows.Forms.ListBox DropdownElements;
    }
}