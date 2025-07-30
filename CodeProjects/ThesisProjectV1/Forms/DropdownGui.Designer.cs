namespace ThesisProjectV1.Forms
{
    partial class DropdownGui
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
            this.dropdownelements = new System.Windows.Forms.ListBox();
            this.textBox = new System.Windows.Forms.TextBox();
            this.exitSelect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // dropdownelements
            // 
            this.dropdownelements.FormattingEnabled = true;
            this.dropdownelements.HorizontalScrollbar = true;
            this.dropdownelements.Location = new System.Drawing.Point(118, 82);
            this.dropdownelements.Name = "dropdownelements";
            this.dropdownelements.Size = new System.Drawing.Size(205, 147);
            this.dropdownelements.TabIndex = 0;
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(32, 24);
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.Size = new System.Drawing.Size(291, 20);
            this.textBox.TabIndex = 1;
            // 
            // exitSelect
            // 
            this.exitSelect.Location = new System.Drawing.Point(32, 82);
            this.exitSelect.Name = "exitSelect";
            this.exitSelect.Size = new System.Drawing.Size(80, 27);
            this.exitSelect.TabIndex = 2;
            this.exitSelect.Text = "Confirm";
            this.exitSelect.UseVisualStyleBackColor = true;
            this.exitSelect.Click += new System.EventHandler(this.ExitSelect_Click);
            // 
            // DropdownGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 261);
            this.Controls.Add(this.exitSelect);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.dropdownelements);
            this.MaximizeBox = false;
            this.Name = "DropdownGui";
            this.Text = "DropdownGui";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox dropdownelements;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.Button exitSelect;
    }
}