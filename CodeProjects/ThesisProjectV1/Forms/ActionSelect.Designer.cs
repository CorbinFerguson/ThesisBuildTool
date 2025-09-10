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
            this.ExitButton = new System.Windows.Forms.Button();
            this.ModifySelect = new System.Windows.Forms.Button();
            this.DeleteSelect = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.LoadButton = new System.Windows.Forms.Button();
            this.NewFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ImportSelect
            // 
            this.ImportSelect.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImportSelect.Location = new System.Drawing.Point(32, 82);
            this.ImportSelect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ImportSelect.Name = "ImportSelect";
            this.ImportSelect.Size = new System.Drawing.Size(136, 115);
            this.ImportSelect.TabIndex = 0;
            this.ImportSelect.Text = "Import Element";
            this.ImportSelect.UseVisualStyleBackColor = true;
            this.ImportSelect.Click += new System.EventHandler(this.ImportSelect_Click);
            // 
            // GenerateSelect
            // 
            this.GenerateSelect.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GenerateSelect.Location = new System.Drawing.Point(174, 82);
            this.GenerateSelect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GenerateSelect.Name = "GenerateSelect";
            this.GenerateSelect.Size = new System.Drawing.Size(136, 115);
            this.GenerateSelect.TabIndex = 1;
            this.GenerateSelect.Text = "Create from Template";
            this.GenerateSelect.UseVisualStyleBackColor = true;
            this.GenerateSelect.Click += new System.EventHandler(this.GenerateSelect_Click);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(32, 11);
            this.textBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(562, 42);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "Select the action you wish to take:";
            // 
            // ExitButton
            // 
            this.ExitButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExitButton.Location = new System.Drawing.Point(305, 219);
            this.ExitButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(96, 55);
            this.ExitButton.TabIndex = 3;
            this.ExitButton.Text = "Exit";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // ModifySelect
            // 
            this.ModifySelect.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ModifySelect.Location = new System.Drawing.Point(316, 82);
            this.ModifySelect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ModifySelect.Name = "ModifySelect";
            this.ModifySelect.Size = new System.Drawing.Size(136, 115);
            this.ModifySelect.TabIndex = 4;
            this.ModifySelect.Text = "Modify Element";
            this.ModifySelect.UseVisualStyleBackColor = true;
            this.ModifySelect.Click += new System.EventHandler(this.ModifySelect_Click);
            // 
            // DeleteSelect
            // 
            this.DeleteSelect.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeleteSelect.Location = new System.Drawing.Point(458, 82);
            this.DeleteSelect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.DeleteSelect.Name = "DeleteSelect";
            this.DeleteSelect.Size = new System.Drawing.Size(136, 115);
            this.DeleteSelect.TabIndex = 5;
            this.DeleteSelect.Text = "Delete Element";
            this.DeleteSelect.UseVisualStyleBackColor = true;
            this.DeleteSelect.Click += new System.EventHandler(this.DeleteSelect_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveButton.Location = new System.Drawing.Point(101, 219);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(96, 55);
            this.SaveButton.TabIndex = 6;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoadButton.Location = new System.Drawing.Point(203, 219);
            this.LoadButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(96, 55);
            this.LoadButton.TabIndex = 7;
            this.LoadButton.Text = "Load";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // NewFile
            // 
            this.NewFile.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewFile.Location = new System.Drawing.Point(407, 219);
            this.NewFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.NewFile.Name = "NewFile";
            this.NewFile.Size = new System.Drawing.Size(96, 55);
            this.NewFile.TabIndex = 8;
            this.NewFile.Text = "New";
            this.NewFile.UseVisualStyleBackColor = true;
            this.NewFile.Click += new System.EventHandler(this.NewFile_Click);
            // 
            // ActionSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(633, 343);
            this.Controls.Add(this.NewFile);
            this.Controls.Add(this.LoadButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.DeleteSelect);
            this.Controls.Add(this.ModifySelect);
            this.Controls.Add(this.ExitButton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.GenerateSelect);
            this.Controls.Add(this.ImportSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "ActionSelect";
            this.Text = "ActionSelect";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ImportSelect;
        private System.Windows.Forms.Button GenerateSelect;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Button ModifySelect;
        private System.Windows.Forms.Button DeleteSelect;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.Button NewFile;
    }
}