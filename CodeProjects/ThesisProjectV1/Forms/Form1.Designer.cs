namespace ThesisProjectV1
{
    partial class EndSelectionForm
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
            this.buttonYes = new System.Windows.Forms.Button();
            this.buttonNo = new System.Windows.Forms.Button();
            this.endSelectionText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonYes
            // 
            this.buttonYes.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonYes.Location = new System.Drawing.Point(36, 62);
            this.buttonYes.Margin = new System.Windows.Forms.Padding(2);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(70, 38);
            this.buttonYes.TabIndex = 0;
            this.buttonYes.Text = "Yes";
            this.buttonYes.UseVisualStyleBackColor = true;
            // 
            // buttonNo
            // 
            this.buttonNo.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonNo.Location = new System.Drawing.Point(118, 62);
            this.buttonNo.Margin = new System.Windows.Forms.Padding(2);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(70, 38);
            this.buttonNo.TabIndex = 1;
            this.buttonNo.Text = "No";
            this.buttonNo.UseVisualStyleBackColor = true;
            this.buttonNo.Click += new System.EventHandler(this.button2_Click);
            // 
            // endSelectionText
            // 
            this.endSelectionText.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.endSelectionText.Location = new System.Drawing.Point(36, 12);
            this.endSelectionText.Name = "endSelectionText";
            this.endSelectionText.Size = new System.Drawing.Size(152, 20);
            this.endSelectionText.TabIndex = 2;
            this.endSelectionText.Text = "Do you wish to end selection?";
            this.endSelectionText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.endSelectionText.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // EndSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 111);
            this.Controls.Add(this.endSelectionText);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.buttonYes);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EndSelectionForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.Button buttonNo;
        private System.Windows.Forms.TextBox endSelectionText;
    }
}