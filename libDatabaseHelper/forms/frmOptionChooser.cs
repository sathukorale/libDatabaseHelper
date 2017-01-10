using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using libDatabaseHelper.classes.generic;

namespace libDatabaseHelper.forms
{
    public partial class frmOptionChooser : Form
    {
        private static frmOptionChooser _presented_form = null;
        private string _selectedOption = null;

        public frmOptionChooser()
        {
            InitializeComponent();
            FormUtils.MakeControlAndSubControlsSensitiveToKey(this, new[] { Keys.Escape, Keys.Enter }, (key) =>
            {
                if (key == Keys.Enter)
                {
                    btnOk_Click(btnOK, EventArgs.Empty);
                }
                else if (key == Keys.Escape)
                {
                    btnCancel_Click(btnClose, EventArgs.Empty);
                }
                else
                {
                    return false;
                }
                return true;
            });
        }

        private void Init(string prompt, string title, string[] options, string defaultOption = null)
        {
            Text = title;
            lblPrompt.Text = prompt;
            _selectedOption = null;
            cmbOptions.Items.Clear();
            cmbOptions.Items.AddRange(options.Select(i => i.Trim()).ToArray());
            cmbOptions.SelectedIndex = 0;

            if (defaultOption != null)
            {
                var index = cmbOptions.Items.IndexOf(defaultOption.Trim());
                if (index != -1)
                {
                    cmbOptions.SelectedIndex = index;
                }
            }
        }

        public static string Show(string prompt, string title, string[] options, string defaulOption = null, IWin32Window owner = null)
        {
            if (_presented_form != null && !_presented_form.IsDisposed)
            {
                _presented_form.Close();
            }
            _presented_form = new frmOptionChooser();
            _presented_form.Init(prompt, title, options, defaulOption);
            _presented_form.ShowDialog(owner);
            return _presented_form._selectedOption;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _selectedOption = cmbOptions.SelectedItem as string;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _selectedOption = null;
            Close();
        }
    }
}
