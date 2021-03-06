﻿namespace libDatabaseHelper.forms
{
    partial class DatabaseEntityViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseEntityViewer));
            this.btnAddDatabaseEntity = new System.Windows.Forms.Button();
            this.btnRemoveDatabaseEntity = new System.Windows.Forms.Button();
            this.dgvDatabaseEntities = new System.Windows.Forms.DataGridView();
            this.btnCopyEntity = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabaseEntities)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAddDatabaseEntity
            // 
            this.btnAddDatabaseEntity.BackgroundImage = global::libDatabaseHelper.Properties.Resources.ico_addrecord;
            this.btnAddDatabaseEntity.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnAddDatabaseEntity.Location = new System.Drawing.Point(8, 8);
            this.btnAddDatabaseEntity.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnAddDatabaseEntity.Name = "btnAddDatabaseEntity";
            this.btnAddDatabaseEntity.Size = new System.Drawing.Size(33, 33);
            this.btnAddDatabaseEntity.TabIndex = 0;
            this.btnAddDatabaseEntity.UseVisualStyleBackColor = true;
            this.btnAddDatabaseEntity.Click += new System.EventHandler(this.btnAddDatabaseEntity_Click);
            // 
            // btnRemoveDatabaseEntity
            // 
            this.btnRemoveDatabaseEntity.BackgroundImage = global::libDatabaseHelper.Properties.Resources.ico_removerecord;
            this.btnRemoveDatabaseEntity.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnRemoveDatabaseEntity.Location = new System.Drawing.Point(44, 8);
            this.btnRemoveDatabaseEntity.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnRemoveDatabaseEntity.Name = "btnRemoveDatabaseEntity";
            this.btnRemoveDatabaseEntity.Size = new System.Drawing.Size(33, 33);
            this.btnRemoveDatabaseEntity.TabIndex = 1;
            this.btnRemoveDatabaseEntity.UseVisualStyleBackColor = true;
            this.btnRemoveDatabaseEntity.Click += new System.EventHandler(this.btnRemoveDatabaseEntity_Click);
            // 
            // dgvDatabaseEntities
            // 
            this.dgvDatabaseEntities.AllowUserToAddRows = false;
            this.dgvDatabaseEntities.AllowUserToDeleteRows = false;
            this.dgvDatabaseEntities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDatabaseEntities.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDatabaseEntities.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvDatabaseEntities.Location = new System.Drawing.Point(8, 49);
            this.dgvDatabaseEntities.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgvDatabaseEntities.Name = "dgvDatabaseEntities";
            this.dgvDatabaseEntities.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDatabaseEntities.Size = new System.Drawing.Size(512, 205);
            this.dgvDatabaseEntities.TabIndex = 2;
            this.dgvDatabaseEntities.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDatabaseEntities_CellDoubleClick);
            // 
            // btnCopyEntity
            // 
            this.btnCopyEntity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyEntity.BackgroundImage = global::libDatabaseHelper.Properties.Resources.ico_copyentity;
            this.btnCopyEntity.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnCopyEntity.Location = new System.Drawing.Point(487, 8);
            this.btnCopyEntity.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCopyEntity.Name = "btnCopyEntity";
            this.btnCopyEntity.Size = new System.Drawing.Size(33, 33);
            this.btnCopyEntity.TabIndex = 0;
            this.btnCopyEntity.UseVisualStyleBackColor = true;
            this.btnCopyEntity.Click += new System.EventHandler(this.btnCopyEntity_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(423, 262);
            this.btnClose.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(97, 26);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // DatabaseEntityViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(527, 296);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dgvDatabaseEntities);
            this.Controls.Add(this.btnRemoveDatabaseEntity);
            this.Controls.Add(this.btnCopyEntity);
            this.Controls.Add(this.btnAddDatabaseEntity);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DatabaseEntityViewer";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Entity Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDatabaseEntityViewer_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatabaseEntities)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnAddDatabaseEntity;
        private System.Windows.Forms.Button btnRemoveDatabaseEntity;
        private System.Windows.Forms.DataGridView dgvDatabaseEntities;
        private System.Windows.Forms.Button btnCopyEntity;
        private System.Windows.Forms.Button btnClose;
    }
}