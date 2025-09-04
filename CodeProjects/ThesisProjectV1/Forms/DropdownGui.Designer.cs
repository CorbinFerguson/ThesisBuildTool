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
            this.Dropdownelements = new System.Windows.Forms.ListBox();
            this.textBox = new System.Windows.Forms.TextBox();
            this.ExitSelect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Dropdownelements
            // 
            this.Dropdownelements.FormattingEnabled = true;
            this.Dropdownelements.HorizontalScrollbar = true;
            this.Dropdownelements.ItemHeight = 16;
            this.Dropdownelements.Location = new System.Drawing.Point(157, 101);
            this.Dropdownelements.Margin = new System.Windows.Forms.Padding(4);
            this.Dropdownelements.Name = "Dropdownelements";
            this.Dropdownelements.Size = new System.Drawing.Size(272, 180);
            this.Dropdownelements.Sorted = true;
            this.Dropdownelements.TabIndex = 0;
            this.Dropdownelements.DoubleClick += new System.EventHandler(this.Dropdownelements_DoubleClick);
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(43, 30);
            this.textBox.Margin = new System.Windows.Forms.Padding(4);
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.Size = new System.Drawing.Size(387, 22);
            this.textBox.TabIndex = 1;
            // 
            // ExitSelect
            // 
            this.ExitSelect.Location = new System.Drawing.Point(43, 101);
            this.ExitSelect.Margin = new System.Windows.Forms.Padding(4);
            this.ExitSelect.Name = "ExitSelect";
            this.ExitSelect.Size = new System.Drawing.Size(107, 33);
            this.ExitSelect.TabIndex = 2;
            this.ExitSelect.Text = "Confirm";
            this.ExitSelect.UseVisualStyleBackColor = true;
            this.ExitSelect.Click += new System.EventHandler(this.ExitSelect_Click);
            // 
            // DropdownGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 321);
            this.Controls.Add(this.ExitSelect);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.Dropdownelements);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "DropdownGui";
            this.Text = "DropdownGui";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DropdownGui_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox Dropdownelements;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.Button ExitSelect;
    }
}