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
            this.ActionSelectHeader = new System.Windows.Forms.TextBox();
            this.ExitButton = new System.Windows.Forms.Button();
            this.ModifySelect = new System.Windows.Forms.Button();
            this.DeleteSelect = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.LoadButton = new System.Windows.Forms.Button();
            this.NewFile = new System.Windows.Forms.Button();
            this.ValidatorButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ImportSelect
            // 
            this.ImportSelect.AccessibleName = "ImportElementButton";
            this.ImportSelect.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImportSelect.Location = new System.Drawing.Point(24, 66);
            this.ImportSelect.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.ImportSelect.Name = "ImportSelect";
            this.ImportSelect.Size = new System.Drawing.Size(102, 94);
            this.ImportSelect.TabIndex = 0;
            this.ImportSelect.Text = "Import Element";
            this.ImportSelect.UseVisualStyleBackColor = true;
            this.ImportSelect.Click += new System.EventHandler(this.ImportSelect_Click);
            // 
            // GenerateSelect
            // 
            this.GenerateSelect.AccessibleName = "CreateElementButton";
            this.GenerateSelect.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GenerateSelect.Location = new System.Drawing.Point(131, 66);
            this.GenerateSelect.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.GenerateSelect.Name = "GenerateSelect";
            this.GenerateSelect.Size = new System.Drawing.Size(102, 94);
            this.GenerateSelect.TabIndex = 1;
            this.GenerateSelect.Text = "Create Element";
            this.GenerateSelect.UseVisualStyleBackColor = true;
            this.GenerateSelect.Click += new System.EventHandler(this.GenerateSelect_Click);
            // 
            // ActionSelectHeader
            // 
            this.ActionSelectHeader.AccessibleName = "ActionSelectHeader";
            this.ActionSelectHeader.CausesValidation = false;
            this.ActionSelectHeader.Cursor = System.Windows.Forms.Cursors.Default;
            this.ActionSelectHeader.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ActionSelectHeader.Location = new System.Drawing.Point(24, 9);
            this.ActionSelectHeader.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.ActionSelectHeader.Name = "ActionSelectHeader";
            this.ActionSelectHeader.ReadOnly = true;
            this.ActionSelectHeader.Size = new System.Drawing.Size(423, 35);
            this.ActionSelectHeader.TabIndex = 2;
            this.ActionSelectHeader.Text = "Select the action you wish to take:";
            // 
            // ExitButton
            // 
            this.ExitButton.AccessibleName = "ExitActionSelect";
            this.ExitButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExitButton.Location = new System.Drawing.Point(200, 226);
            this.ExitButton.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(72, 45);
            this.ExitButton.TabIndex = 3;
            this.ExitButton.Text = "Exit";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // ModifySelect
            // 
            this.ModifySelect.AccessibleName = "ModifyElementButton";
            this.ModifySelect.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ModifySelect.Location = new System.Drawing.Point(237, 66);
            this.ModifySelect.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.ModifySelect.Name = "ModifySelect";
            this.ModifySelect.Size = new System.Drawing.Size(102, 94);
            this.ModifySelect.TabIndex = 4;
            this.ModifySelect.Text = "Modify Element (WIP)";
            this.ModifySelect.UseVisualStyleBackColor = true;
            this.ModifySelect.Click += new System.EventHandler(this.ModifySelect_Click);
            // 
            // DeleteSelect
            // 
            this.DeleteSelect.AccessibleName = "DeleteElementButton";
            this.DeleteSelect.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeleteSelect.Location = new System.Drawing.Point(343, 66);
            this.DeleteSelect.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.DeleteSelect.Name = "DeleteSelect";
            this.DeleteSelect.Size = new System.Drawing.Size(102, 94);
            this.DeleteSelect.TabIndex = 5;
            this.DeleteSelect.Text = "Delete Element";
            this.DeleteSelect.UseVisualStyleBackColor = true;
            this.DeleteSelect.Click += new System.EventHandler(this.DeleteSelect_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.AccessibleName = "SaveFileButton";
            this.SaveButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SaveButton.Location = new System.Drawing.Point(131, 178);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(102, 45);
            this.SaveButton.TabIndex = 6;
            this.SaveButton.Text = "Save File";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // LoadButton
            // 
            this.LoadButton.AccessibleName = "LoadFileButton";
            this.LoadButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoadButton.Location = new System.Drawing.Point(237, 178);
            this.LoadButton.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(102, 45);
            this.LoadButton.TabIndex = 7;
            this.LoadButton.Text = "Load File";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // NewFile
            // 
            this.NewFile.AccessibleName = "NewFileButton";
            this.NewFile.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewFile.Location = new System.Drawing.Point(24, 178);
            this.NewFile.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.NewFile.Name = "NewFile";
            this.NewFile.Size = new System.Drawing.Size(102, 45);
            this.NewFile.TabIndex = 8;
            this.NewFile.Text = "New File";
            this.NewFile.UseVisualStyleBackColor = true;
            this.NewFile.Click += new System.EventHandler(this.NewButton_Click);
            // 
            // ValidatorButton
            // 
            this.ValidatorButton.AccessibleName = "ValidateFileButton";
            this.ValidatorButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ValidatorButton.Location = new System.Drawing.Point(343, 178);
            this.ValidatorButton.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.ValidatorButton.Name = "ValidatorButton";
            this.ValidatorButton.Size = new System.Drawing.Size(102, 45);
            this.ValidatorButton.TabIndex = 9;
            this.ValidatorButton.Text = "Validate";
            this.ValidatorButton.UseVisualStyleBackColor = true;
            this.ValidatorButton.Click += new System.EventHandler(this.ValidateButton_Click);
            // 
            // ActionSelect
            // 
            this.AccessibleName = "ActionSelector";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 279);
            this.Controls.Add(this.ValidatorButton);
            this.Controls.Add(this.NewFile);
            this.Controls.Add(this.LoadButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.DeleteSelect);
            this.Controls.Add(this.ModifySelect);
            this.Controls.Add(this.ExitButton);
            this.Controls.Add(this.ActionSelectHeader);
            this.Controls.Add(this.GenerateSelect);
            this.Controls.Add(this.ImportSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.MaximizeBox = false;
            this.Name = "ActionSelect";
            this.Text = "ActionSelect";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ImportSelect;
        private System.Windows.Forms.Button GenerateSelect;
        private System.Windows.Forms.TextBox ActionSelectHeader;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Button ModifySelect;
        private System.Windows.Forms.Button DeleteSelect;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.Button NewFile;
        private System.Windows.Forms.Button ValidatorButton;
    }
}