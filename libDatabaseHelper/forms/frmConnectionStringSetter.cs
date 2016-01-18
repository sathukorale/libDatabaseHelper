using System;
using System.Windows.Forms;
using libDatabaseHelper.classes.generic;

namespace libDatabaseHelper.forms
{
    public partial class frmConnectionStringSetter : Form
    {
        private static frmConnectionStringSetter _presentedForm;
        private static bool _isValid;
        public frmConnectionStringSetter()
        {
            InitializeComponent();
        }

        private void ClearForm()
        {
            FormUtils.ClearForm(this);
        }

        public static bool ShowWindow()
        {
            if (_presentedForm == null || _presentedForm.IsDisposed)
                _presentedForm = new frmConnectionStringSetter();
            _isValid = false;
            _presentedForm.ClearForm();
            _presentedForm.ShowDialog();
            return _isValid;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _isValid = false;
            ClearForm();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (txtConnectionString.Text.Trim() != "") return;
            _isValid = false;
            MessageBox.Show(this, "The application cannot start without the connection string set. The application will now exit.", "Connection String Required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Application.Exit();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtConnectionString.Text.Trim() == "")
            {
                MessageBox.Show(this, "Please enter the connection string. Otherwise the program will not be able to connect to the database.", "Connection String Required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (ConnectionManager.CheckConnectionString(txtConnectionString.Text))
            {
                _isValid = true;
                ConnectionManager.SetConnectionString(txtConnectionString.Text);
                MessageBox.Show(this, "The application was configured with the connection string successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Hide();
            }
            else
            {
                MessageBox.Show(this, "The application was unable to connect to the database specified by the connection string", "Invalid Connection String", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void frmConnectionStringSetter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (txtConnectionString.Text.Trim() != "") return;
            _isValid = false;
            MessageBox.Show(this, "The application cannot start without the connection string set. The application will now exit.", "Connection String Required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Environment.Exit(0);
        }
    }
}
