namespace ThesisProjectV1.Forms
{
    partial class ActionSelect
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
            this.ImportSelect = new System.Windows.Forms.Button();
            this.GenerateSelect = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ImportSelect
            // 
            this.ImportSelect.Location = new System.Drawing.Point(50, 75);
            this.ImportSelect.Name = "ImportSelect";
            this.ImportSelect.Size = new System.Drawing.Size(183, 100);
            this.ImportSelect.TabIndex = 0;
            this.ImportSelect.Text = "Import Element";
            this.ImportSelect.UseVisualStyleBackColor = true;
            this.ImportSelect.Click += new System.EventHandler(this.ImportSelect_Click);
            // 
            // GenerateSelect
            // 
            this.GenerateSelect.Location = new System.Drawing.Point(300, 75);
            this.GenerateSelect.Name = "GenerateSelect";
            this.GenerateSelect.Size = new System.Drawing.Size(183, 100);
            this.GenerateSelect.TabIndex = 1;
            this.GenerateSelect.Text = "Generate from Template";
            this.GenerateSelect.UseVisualStyleBackColor = true;
            this.GenerateSelect.Click += new System.EventHandler(this.GenerateSelect_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(50, 25);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(433, 26);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "Select the action you wish to take:";
            // 
            // ActionSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 234);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.GenerateSelect);
            this.Controls.Add(this.ImportSelect);
            this.Name = "ActionSelect";
            this.Text = "ActionSelect";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ImportSelect;
        private System.Windows.Forms.Button GenerateSelect;
        private System.Windows.Forms.TextBox textBox1;
    }
}