namespace libDatabaseHelper.forms
{
    partial class frmConnectionStringSetter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConnectionStringSetter));
            this.pnlControlContainer = new System.Windows.Forms.Panel();
            this.grpCategoryDetails = new System.Windows.Forms.GroupBox();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.lblCountry = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlControlContainer.SuspendLayout();
            this.grpCategoryDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControlContainer
            // 
            this.pnlControlContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlControlContainer.AutoScroll = true;
            this.pnlControlContainer.Controls.Add(this.grpCategoryDetails);
            this.pnlControlContainer.Location = new System.Drawing.Point(0, 0);
            this.pnlControlContainer.Name = "pnlControlContainer";
            this.pnlControlContainer.Size = new System.Drawing.Size(516, 100);
            this.pnlControlContainer.TabIndex = 20;
            // 
            // grpCategoryDetails
            // 
            this.grpCategoryDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCategoryDetails.Controls.Add(this.txtConnectionString);
            this.grpCategoryDetails.Controls.Add(this.lblCountry);
            this.grpCategoryDetails.Location = new System.Drawing.Point(12, 14);
            this.grpCategoryDetails.Name = "grpCategoryDetails";
            this.grpCategoryDetails.Size = new System.Drawing.Size(491, 61);
            this.grpCategoryDetails.TabIndex = 14;
            this.grpCategoryDetails.TabStop = false;
            this.grpCategoryDetails.Text = "Connection String";
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConnectionString.Location = new System.Drawing.Point(106, 25);
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(369, 20);
            this.txtConnectionString.TabIndex = 14;
            // 
            // lblCountry
            // 
            this.lblCountry.AutoSize = true;
            this.lblCountry.Location = new System.Drawing.Point(6, 28);
            this.lblCountry.Name = "lblCountry";
            this.lblCountry.Size = new System.Drawing.Size(94, 13);
            this.lblCountry.TabIndex = 13;
            this.lblCountry.Text = "Connection String ";
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.Location = new System.Drawing.Point(13, 112);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(91, 27);
            this.btnReset.TabIndex = 23;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(316, 112);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(91, 27);
            this.btnOK.TabIndex = 22;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(413, 112);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(91, 27);
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // frmConnectionStringSetter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 151);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.pnlControlContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmConnectionStringSetter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connection String";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmConnectionStringSetter_FormClosing);
            this.pnlControlContainer.ResumeLayout(false);
            this.grpCategoryDetails.ResumeLayout(false);
            this.grpCategoryDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControlContainer;
        private System.Windows.Forms.GroupBox grpCategoryDetails;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label lblCountry;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;

    }
}