﻿namespace libDatabaseHelper.forms.controls
{
    partial class NamedDateTimePicker
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblName = new System.Windows.Forms.Label();
            this.btnRemoveControl = new System.Windows.Forms.Button();
            this.dtpCalendar = new System.Windows.Forms.DateTimePicker();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMain.Controls.Add(this.lblName, 0, 0);
            this.tlpMain.Controls.Add(this.btnRemoveControl, 2, 0);
            this.tlpMain.Controls.Add(this.dtpCalendar, 1, 0);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 0);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Size = new System.Drawing.Size(349, 29);
            this.tlpMain.TabIndex = 0;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(3, 0);
            this.lblName.Name = "lblName";
            this.lblName.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lblName.Size = new System.Drawing.Size(77, 29);
            this.lblName.TabIndex = 3;
            this.lblName.Text = "Item Name";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnRemoveControl
            // 
            this.btnRemoveControl.BackgroundImage = global::libDatabaseHelper.Properties.Resources.ico_remove_field;
            this.btnRemoveControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnRemoveControl.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveControl.FlatAppearance.BorderSize = 0;
            this.btnRemoveControl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveControl.Location = new System.Drawing.Point(323, 3);
            this.btnRemoveControl.Name = "btnRemoveControl";
            this.btnRemoveControl.Size = new System.Drawing.Size(23, 23);
            this.btnRemoveControl.TabIndex = 4;
            this.btnRemoveControl.TabStop = false;
            this.btnRemoveControl.UseVisualStyleBackColor = true;
            // 
            // dtpCalendar
            // 
            this.dtpCalendar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtpCalendar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dtpCalendar.Location = new System.Drawing.Point(86, 3);
            this.dtpCalendar.Name = "dtpCalendar";
            this.dtpCalendar.Size = new System.Drawing.Size(231, 23);
            this.dtpCalendar.TabIndex = 5;
            // 
            // NamedDateTimePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlpMain);
            this.Name = "NamedDateTimePicker";
            this.Size = new System.Drawing.Size(349, 29);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Button btnRemoveControl;
        private System.Windows.Forms.DateTimePicker dtpCalendar;
    }
}
