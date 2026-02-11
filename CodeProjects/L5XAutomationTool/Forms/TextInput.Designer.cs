namespace L5XAutomationTool.Forms
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // UserInstructionTextBox
            // 
            this.UserInstructionTextBox.AccessibleName = "Title";
            this.UserInstructionTextBox.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
            this.UserInstructionTextBox.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.tableLayoutPanel1.SetColumnSpan(this.UserInstructionTextBox, 2);
            this.UserInstructionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UserInstructionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UserInstructionTextBox.Location = new System.Drawing.Point(2, 2);
            this.UserInstructionTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 4, 2);
            this.UserInstructionTextBox.Name = "UserInstructionTextBox";
            this.UserInstructionTextBox.ReadOnly = true;
            this.UserInstructionTextBox.Size = new System.Drawing.Size(468, 26);
            this.UserInstructionTextBox.TabIndex = 0;
            this.UserInstructionTextBox.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // InputValueName
            // 
            this.InputValueName.AccessibleName = "TextField";
            this.InputValueName.AccessibleRole = System.Windows.Forms.AccessibleRole.Text;
            this.InputValueName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InputValueName.Location = new System.Drawing.Point(80, 32);
            this.InputValueName.Margin = new System.Windows.Forms.Padding(2, 2, 4, 2);
            this.InputValueName.Name = "InputValueName";
            this.InputValueName.Size = new System.Drawing.Size(390, 20);
            this.InputValueName.TabIndex = 2;
            this.InputValueName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputValueName_KeyDown);
            // 
            // SubmitButton
            // 
            this.SubmitButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SubmitButton.AutoSize = true;
            this.SubmitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SubmitButton.Location = new System.Drawing.Point(2, 32);
            this.SubmitButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.SubmitButton.Name = "SubmitButton";
            this.SubmitButton.Size = new System.Drawing.Size(74, 30);
            this.SubmitButton.TabIndex = 3;
            this.SubmitButton.Text = "Submit";
            this.SubmitButton.UseVisualStyleBackColor = true;
            this.SubmitButton.Click += new System.EventHandler(this.SubmitButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.SubmitButton, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.InputValueName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.UserInstructionTextBox, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(473, 102);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // TextInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 102);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "TextInput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TextInput";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox UserInstructionTextBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TextBox InputValueName;
        private System.Windows.Forms.Button SubmitButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}