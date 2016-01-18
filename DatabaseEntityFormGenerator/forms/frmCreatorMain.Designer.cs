namespace DatabaseEntityFormCreator
{
    partial class frmCreatorMain
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
            this.btnCreateForm = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCreateForm
            // 
            this.btnCreateForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateForm.Location = new System.Drawing.Point(14, 16);
            this.btnCreateForm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCreateForm.Name = "btnCreateForm";
            this.btnCreateForm.Size = new System.Drawing.Size(303, 30);
            this.btnCreateForm.TabIndex = 0;
            this.btnCreateForm.Text = "Create Form(s)";
            this.btnCreateForm.UseVisualStyleBackColor = true;
            this.btnCreateForm.Click += new System.EventHandler(this.button1_Click);
            // 
            // frmCreatorMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 61);
            this.Controls.Add(this.btnCreateForm);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmCreatorMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form Creator";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCreateForm;
    }
}

