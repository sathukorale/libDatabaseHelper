namespace libDatabaseHelper.forms
{
    partial class frmOptionChooser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmOptionChooser));
            this.tlpMainContainer = new System.Windows.Forms.TableLayoutPanel();
            this.flpButtonContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.cmbOptions = new System.Windows.Forms.ComboBox();
            this.lblPrompt = new System.Windows.Forms.Label();
            this.tlpInputContainer = new System.Windows.Forms.TableLayoutPanel();
            this.tlpMainContainer.SuspendLayout();
            this.flpButtonContainer.SuspendLayout();
            this.tlpInputContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMainContainer
            // 
            this.tlpMainContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tlpMainContainer.ColumnCount = 1;
            this.tlpMainContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMainContainer.Controls.Add(this.tlpInputContainer, 0, 0);
            this.tlpMainContainer.Controls.Add(this.flpButtonContainer, 0, 1);
            this.tlpMainContainer.Location = new System.Drawing.Point(8, 8);
            this.tlpMainContainer.Name = "tlpMainContainer";
            this.tlpMainContainer.RowCount = 2;
            this.tlpMainContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMainContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMainContainer.Size = new System.Drawing.Size(469, 76);
            this.tlpMainContainer.TabIndex = 5;
            // 
            // flpButtonContainer
            // 
            this.flpButtonContainer.Controls.Add(this.btnClose);
            this.flpButtonContainer.Controls.Add(this.btnOK);
            this.flpButtonContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpButtonContainer.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpButtonContainer.Location = new System.Drawing.Point(3, 39);
            this.flpButtonContainer.Name = "flpButtonContainer";
            this.flpButtonContainer.Size = new System.Drawing.Size(463, 34);
            this.flpButtonContainer.TabIndex = 4;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(385, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(304, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 28);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cmbOptions
            // 
            this.cmbOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOptions.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbOptions.FormattingEnabled = true;
            this.cmbOptions.Location = new System.Drawing.Point(60, 3);
            this.cmbOptions.Name = "cmbOptions";
            this.cmbOptions.Size = new System.Drawing.Size(402, 25);
            this.cmbOptions.TabIndex = 0;
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPrompt.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrompt.Location = new System.Drawing.Point(3, 0);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(51, 32);
            this.lblPrompt.TabIndex = 1;
            this.lblPrompt.Text = "Prompt";
            this.lblPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tlpInputContainer
            // 
            this.tlpInputContainer.ColumnCount = 2;
            this.tlpInputContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpInputContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpInputContainer.Controls.Add(this.lblPrompt, 0, 0);
            this.tlpInputContainer.Controls.Add(this.cmbOptions, 1, 0);
            this.tlpInputContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpInputContainer.Location = new System.Drawing.Point(2, 2);
            this.tlpInputContainer.Margin = new System.Windows.Forms.Padding(2);
            this.tlpInputContainer.Name = "tlpInputContainer";
            this.tlpInputContainer.RowCount = 1;
            this.tlpInputContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpInputContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpInputContainer.Size = new System.Drawing.Size(465, 32);
            this.tlpInputContainer.TabIndex = 3;
            // 
            // frmOptionChooser
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(481, 88);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMainContainer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmOptionChooser";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "<Title>";
            this.tlpMainContainer.ResumeLayout(false);
            this.flpButtonContainer.ResumeLayout(false);
            this.tlpInputContainer.ResumeLayout(false);
            this.tlpInputContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMainContainer;
        private System.Windows.Forms.FlowLayoutPanel flpButtonContainer;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TableLayoutPanel tlpInputContainer;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.ComboBox cmbOptions;

    }
}