using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using libDatabaseHelper.classes.generic;

namespace libDatabaseHelper.forms
{
    public partial class frmInputDialog : Form
    {
        private static frmInputDialog _presented_form = null;
        private static string _value = "";
        private frmInputDialog()
        {
            InitializeComponent();
            FormUtils.MakeControlAndSubControlsSensitiveToKey(this, new[] { Keys.Escape, Keys.Enter }, (key) => 
            {
                if (key == Keys.Enter)
                {
                    btnOK_Click(btnOK, EventArgs.Empty);
                }
                else
                {
                    btnClose_Click(btnClose, EventArgs.Empty);
                }
                return 0; 
            });
        }

        private void Init(string prompt, string title, string default_value)
        {
            Text = title;
            lblPrompt.Text = prompt;
            _value = txtInput.Text = default_value;

            txtInput.Select();

            if (default_value == null || default_value.Trim() == "")
            {
                var text_in_clipboard = Clipboard.GetText();
                if (text_in_clipboard != null && text_in_clipboard.Trim() != null)
                {
                    txtInput.Text = text_in_clipboard;
                    txtInput.SelectAll();
                }
            }
            txtInput.BringToFront();
            Width = txtInput.Width + lblPrompt.Size.Width + 42;
            Height = 120;
            Invalidate();
            Refresh();
        }

        public static string Show(string prompt, string title, string default_value)
        {
            if (_presented_form != null && !_presented_form.IsDisposed)
            {
                _presented_form.Close();
            }
            _presented_form = new frmInputDialog();
            _presented_form.Init(prompt, title, default_value);
            _presented_form.ShowDialog();
            return _value;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _value = txtInput.Text;
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            _value = "";
            Close();
        }

        private void txtInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _value = txtInput.Text;
                Close();
            }
        }
    }
}
