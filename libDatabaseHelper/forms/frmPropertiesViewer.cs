using System;
using System.Windows.Forms;
using libDatabaseHelper.classes.generic;
using libDatabaseHelper.forms.controls;

namespace libDatabaseHelper.forms
{
    public partial class frmPropertiesViewer : Form
    {
        private static frmPropertiesViewer _presentedForm = null;

        public frmPropertiesViewer()
        {
            InitializeComponent();
        }

        public static void ShowWindow(GenericDatabaseEntity entity)
        {
            if (_presentedForm == null || _presentedForm.IsDisposed)
            {
                _presentedForm = new frmPropertiesViewer();
            }
            _presentedForm.Show();
            _presentedForm.LoadEntity(entity);
        } 

        private void LoadEntity(GenericDatabaseEntity entity)
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
