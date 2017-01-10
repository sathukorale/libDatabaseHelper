using System;
using System.Windows.Forms;
using libDatabaseHelper.classes.generic;
using System.Drawing;

namespace libDatabaseHelper.forms
{
    public partial class frmInputDialog : Form
    {
        private static frmInputDialog _presented_form = null;
        private static string _value = "";
        private static bool _cancellable = true;
        private frmInputDialog()
        {
            InitializeComponent();
            FormUtils.MakeControlAndSubControlsSensitiveToKey(this, new[] { Keys.Escape, Keys.Enter }, (key) => 
            {
                if (key == Keys.Enter)
                {
                    btnOK_Click(btnOK, EventArgs.Empty);
                }
                else if (key == Keys.Escape && _cancellable)
                {
                    btnClose_Click(btnClose, EventArgs.Empty);
                }
                else
                {
                    return false; 
                }
                return true;
            });
        }

        private void Init(string prompt, string title, string default_value, bool password_prompt = false, bool canellable = true)
        {
            Text = title;
            lblPrompt.Text = prompt;
            _value = txtInput.Text = default_value;

            txtInput.PasswordChar = password_prompt ? '●' : '\0';
            txtInput.Select();

            btnClose.Enabled = _cancellable = canellable;

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
        }

        public static string Show(string prompt, string title, string default_value, bool password_prompt = false, bool cancellable = true, IWin32Window owner = null)
        {
            if (_presented_form != null && !_presented_form.IsDisposed)
            {
                _presented_form.Close();
            }
            _presented_form = new frmInputDialog();
            _presented_form.Init(prompt, title, default_value, password_prompt, cancellable);
            _presented_form.ShowDialog(owner);
            return _value;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _value = txtInput.Text;
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            _value = null;
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
