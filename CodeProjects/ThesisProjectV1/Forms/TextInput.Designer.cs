namespace ThesisProjectV1.Forms
{
    partial class TextInput
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
            this.components = new System.ComponentModel.Container();
            this.UserInstructionTextBox = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.InputValueName = new System.Windows.Forms.TextBox();
            this.SubmitButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // UserInstructionTextBox
            // 
            this.UserInstructionTextBox.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.UserInstructionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UserInstructionTextBox.Location = new System.Drawing.Point(3, 10);
            this.UserInstructionTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.UserInstructionTextBox.Name = "UserInstructionTextBox";
            this.UserInstructionTextBox.ReadOnly = true;
            this.UserInstructionTextBox.Size = new System.Drawing.Size(936, 30);
            this.UserInstructionTextBox.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // InputValueName
            // 
            this.InputValueName.Location = new System.Drawing.Point(116, 84);
            this.InputValueName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.InputValueName.Name = "InputValueName";
            this.InputValueName.Size = new System.Drawing.Size(390, 22);
            this.InputValueName.TabIndex = 2;
            this.InputValueName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputValueName_KeyDown);
            // 
            // SubmitButton
            // 
            this.SubmitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubmitButton.Location = new System.Drawing.Point(11, 62);
            this.SubmitButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SubmitButton.Name = "SubmitButton";
            this.SubmitButton.Size = new System.Drawing.Size(100, 62);
            this.SubmitButton.TabIndex = 3;
            this.SubmitButton.Text = "Submit";
            this.SubmitButton.UseVisualStyleBackColor = true;
            this.SubmitButton.Click += new System.EventHandler(this.SubmitButton_Click);
            // 
            // TextInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 142);
            this.Controls.Add(this.SubmitButton);
            this.Controls.Add(this.InputValueName);
            this.Controls.Add(this.UserInstructionTextBox);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "TextInput";
            this.Text = "TextInput";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TextInput_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox UserInstructionTextBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TextBox InputValueName;
        private System.Windows.Forms.Button SubmitButton;
    }
}