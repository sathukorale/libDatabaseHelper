using libDatabaseHelper.classes.generic;

namespace libDatabaseHelperUnitTests.forms
{
    partial class frmSampleTable2
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
			this.btnReset = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.pnlControlContainer = new System.Windows.Forms.Panel();
this.lblColumn2 = new System.Windows.Forms.Label();
this.txtColumn2 = new System.Windows.Forms.TextBox();
this.lblColumn3 = new System.Windows.Forms.Label();
this.cmbColumn3 = new System.Windows.Forms.ComboBox();
			this.pnlControlContainer.SuspendLayout();
			this.SuspendLayout();
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.Location = new System.Drawing.Point(10, 85);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(91, 27);
            this.btnReset.TabIndex = 9;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(98, 85);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(91, 27);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(199, 85);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(91, 27);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
this.lblColumn2.Text = "Column 2";
		this.lblColumn2.Location = new System.Drawing.Point(10, 15);
		this.lblColumn2.Size = new System.Drawing.Size(100, 20);
		this.txtColumn2.Location = new System.Drawing.Point(120, 15);
		this.txtColumn2.Size = new System.Drawing.Size(160, 20);
		this.txtColumn2.TabIndex = 0;
		this.txtColumn2.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left);
			this.txtColumn2.Tag = new TableColumnField(true, typeof(SampleTable2), "Column2");
this.lblColumn3.Text = "Column 3";
		this.lblColumn3.Location = new System.Drawing.Point(10, 45);
		this.lblColumn3.Size = new System.Drawing.Size(100, 20);
		this.cmbColumn3.Location = new System.Drawing.Point(120, 45);
		this.cmbColumn3.Size = new System.Drawing.Size(160, 20);
		this.cmbColumn3.TabIndex = 1;
		this.cmbColumn3.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left);
		this.cmbColumn3.Tag = new TableColumnField(true, typeof(SampleTable2), "Column3");		this.cmbColumn3.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;		this.cmbColumn3.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            // 
            // pnlControlContainer
            // 
            this.pnlControlContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlControlContainer.AutoScroll = true;
            this.pnlControlContainer.Location = new System.Drawing.Point(0, 0);
            this.pnlControlContainer.Name = "pnlControlContainer";
            this.pnlControlContainer.Size = new System.Drawing.Size(300,75);
		this.pnlControlContainer.Controls.Add(lblColumn2);
		this.pnlControlContainer.Controls.Add(txtColumn2);
		this.pnlControlContainer.Controls.Add(lblColumn3);
		this.pnlControlContainer.Controls.Add(cmbColumn3);
            this.pnlControlContainer.TabIndex = 11;
            // 
            // frmSampleTable2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300,122);
            this.Controls.Add(this.pnlControlContainer);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Name = "frmSampleTable2";
            this.Text = "Form1";
            this.pnlControlContainer.ResumeLayout(false);
            this.pnlControlContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

    private System.Windows.Forms.Button btnReset;
	private System.Windows.Forms.Button btnOK;
	private System.Windows.Forms.Button btnCancel;
	private System.Windows.Forms.Panel pnlControlContainer;
	private System.Windows.Forms.Label lblColumn2;
	private System.Windows.Forms.TextBox txtColumn2;
	private System.Windows.Forms.Label lblColumn3;
	private System.Windows.Forms.ComboBox cmbColumn3;
    }
}