using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using libDatabaseHelper.classes.generic;
using libDatabaseHelper.classes.sqlce;
using libDatabaseHelper.forms.controls;

namespace libDatabaseHelper.forms
{
    public partial class frmPropertiesViewer : Form
    {
        private static frmPropertiesViewer _presented_form = null;

        public frmPropertiesViewer()
        {
            InitializeComponent();
        }

        public static void ShowWindow()
        {
            if (_presented_form == null || _presented_form.IsDisposed)
            {
                _presented_form = new frmPropertiesViewer();
            }
            _presented_form.Show();
            _presented_form.LoadEntity();
        } 

        private void LoadEntity()
        {
            dgvProperties.Columns.Clear();
            dgvProperties.Columns.Add(new CalendarColumn());
            dgvProperties.RowCount = 5;
            foreach (DataGridViewRow row in this.dgvProperties.Rows)
            {
                row.Cells[0].Value = DateTime.Now;
            }
            dgvProperties.Invalidate();
        }
    }
}
