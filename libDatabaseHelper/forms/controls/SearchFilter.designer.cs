namespace libDatabaseHelper.forms.controls
{
    partial class SearchFilter
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
            this.components = new System.ComponentModel.Container();
            this.txtMainSearchFilter = new System.Windows.Forms.TextBox();
            this.lnkAdvancedSettings = new System.Windows.Forms.LinkLabel();
            this.btnChangeDefaultFilter = new System.Windows.Forms.Button();
            this.cntxSearchFilters = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnSearch = new System.Windows.Forms.Button();
            this.flpAdvancedSettings = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAddField = new System.Windows.Forms.Button();
            this.cntxAdvancedSearchFilters = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.flpAdvancedSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtMainSearchFilter
            // 
            this.txtMainSearchFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMainSearchFilter.BackColor = System.Drawing.Color.White;
            this.txtMainSearchFilter.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMainSearchFilter.Location = new System.Drawing.Point(4, 4);
            this.txtMainSearchFilter.Name = "txtMainSearchFilter";
            this.txtMainSearchFilter.Size = new System.Drawing.Size(730, 25);
            this.txtMainSearchFilter.TabIndex = 0;
            // 
            // lnkAdvancedSettings
            // 
            this.lnkAdvancedSettings.AutoSize = true;
            this.lnkAdvancedSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkAdvancedSettings.Location = new System.Drawing.Point(3, 33);
            this.lnkAdvancedSettings.Name = "lnkAdvancedSettings";
            this.lnkAdvancedSettings.Size = new System.Drawing.Size(109, 15);
            this.lnkAdvancedSettings.TabIndex = 1;
            this.lnkAdvancedSettings.TabStop = true;
            this.lnkAdvancedSettings.Text = "More Filter Options";
            this.lnkAdvancedSettings.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkAdvancedSettings_LinkClicked);
            // 
            // btnChangeDefaultFilter
            // 
            this.btnChangeDefaultFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChangeDefaultFilter.BackColor = System.Drawing.Color.White;
            this.btnChangeDefaultFilter.BackgroundImage = global::libDatabaseHelper.Properties.Resources.ico_more_options;
            this.btnChangeDefaultFilter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnChangeDefaultFilter.ContextMenuStrip = this.cntxSearchFilters;
            this.btnChangeDefaultFilter.FlatAppearance.BorderSize = 0;
            this.btnChangeDefaultFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChangeDefaultFilter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChangeDefaultFilter.Location = new System.Drawing.Point(705, 6);
            this.btnChangeDefaultFilter.Name = "btnChangeDefaultFilter";
            this.btnChangeDefaultFilter.Size = new System.Drawing.Size(28, 20);
            this.btnChangeDefaultFilter.TabIndex = 2;
            this.btnChangeDefaultFilter.UseVisualStyleBackColor = false;
            this.btnChangeDefaultFilter.Click += new System.EventHandler(this.btnChangeDefaultFilter_Click);
            // 
            // cntxSearchFilters
            // 
            this.cntxSearchFilters.Name = "cntxSearchFilters";
            this.cntxSearchFilters.Size = new System.Drawing.Size(61, 4);
            this.cntxSearchFilters.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.cntxSearchFilters_ItemClicked);
            // 
            // btnSearch
            // 
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSearch.Location = new System.Drawing.Point(738, 4);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(89, 25);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // flpAdvancedSettings
            // 
            this.flpAdvancedSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpAdvancedSettings.AutoScroll = true;
            this.flpAdvancedSettings.BackColor = System.Drawing.Color.Gainsboro;
            this.flpAdvancedSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flpAdvancedSettings.Controls.Add(this.btnAddField);
            this.flpAdvancedSettings.Location = new System.Drawing.Point(3, 54);
            this.flpAdvancedSettings.Margin = new System.Windows.Forms.Padding(0);
            this.flpAdvancedSettings.Name = "flpAdvancedSettings";
            this.flpAdvancedSettings.Size = new System.Drawing.Size(826, 118);
            this.flpAdvancedSettings.TabIndex = 4;
            // 
            // btnAddField
            // 
            this.btnAddField.BackgroundImage = global::libDatabaseHelper.Properties.Resources.ico_add_field;
            this.btnAddField.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnAddField.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddField.FlatAppearance.BorderSize = 0;
            this.btnAddField.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddField.Location = new System.Drawing.Point(3, 3);
            this.btnAddField.Name = "btnAddField";
            this.btnAddField.Size = new System.Drawing.Size(29, 29);
            this.btnAddField.TabIndex = 0;
            this.btnAddField.UseVisualStyleBackColor = true;
            this.btnAddField.Click += new System.EventHandler(this.btnAddField_Click);
            // 
            // cntxAdvancedSearchFilters
            // 
            this.cntxAdvancedSearchFilters.Name = "cntxSearchFilters";
            this.cntxAdvancedSearchFilters.Size = new System.Drawing.Size(61, 4);
            this.cntxAdvancedSearchFilters.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.cntxAdvancedSearchFilters_ItemClicked);
            // 
            // SearchFilter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flpAdvancedSettings);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.btnChangeDefaultFilter);
            this.Controls.Add(this.lnkAdvancedSettings);
            this.Controls.Add(this.txtMainSearchFilter);
            this.Name = "SearchFilter";
            this.Size = new System.Drawing.Size(832, 175);
            this.Load += new System.EventHandler(this.SearchFilter_Load);
            this.flpAdvancedSettings.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtMainSearchFilter;
        private System.Windows.Forms.LinkLabel lnkAdvancedSettings;
        private System.Windows.Forms.Button btnChangeDefaultFilter;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.FlowLayoutPanel flpAdvancedSettings;
        private System.Windows.Forms.ContextMenuStrip cntxSearchFilters;
        private System.Windows.Forms.Button btnAddField;
        private System.Windows.Forms.ContextMenuStrip cntxAdvancedSearchFilters;
    }
}
