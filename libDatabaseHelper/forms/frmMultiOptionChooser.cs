using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace libDatabaseHelper.forms
{
    public partial class frmMultiOptionChooser : Form
    {
        private static string[] _selectedOptions = null;
        private static string[] _disabledOptions = null;
        private static bool _enableMultipleChoices = false;

        private frmMultiOptionChooser()
        {
            InitializeComponent();
        }

        private void Setup(string title, string description, string[] options, string[] selectedOptions = null, string[] disabledOptions = null, bool cancellable = true, bool enableMultipleChoices = false)
        {
            Text = title;
            lblDescription.Text = description;
            btnCancel.Enabled = cancellable;

            _selectedOptions = selectedOptions ?? new string[0];
            _disabledOptions = disabledOptions ?? new string[0];
            _enableMultipleChoices = enableMultipleChoices;

            lstOptions.Items.Clear();
            lstOptions.Items.AddRange(options);

            foreach (var option in _selectedOptions)
            { 
                try
                {
                    var index = lstOptions.Items.IndexOf(option);
                    if (index != -1)
                    {
                        lstOptions.SetItemChecked(index, true);
                    }
                }
                catch {}
            }

            foreach (var option in _disabledOptions)
            {
                try
                {
                    var index = lstOptions.Items.IndexOf(option);
                    if (index != -1)
                    {
                        lstOptions.SetItemCheckState(index, CheckState.Indeterminate);
                    }
                }
                catch { }
            }
        }

        public static string[] ShowWindow(string title, string description, string[] options, string[] selectedOptions = null, string[] disabledOptions = null, bool cancellable = true, bool enableMultipleChoices = false, IWin32Window owner = null)
        {
            var presentedDialog = new frmMultiOptionChooser();
            presentedDialog.Setup(title, description, options, selectedOptions, disabledOptions, cancellable, enableMultipleChoices);
            presentedDialog.ShowDialog();
            presentedDialog.Close();

            return _selectedOptions;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _selectedOptions = null;
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _selectedOptions = lstOptions.CheckedItems.OfType<string>().ToArray();
            Close();
        }

        private void lstOptions_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue == CheckState.Indeterminate)
            {
                e.NewValue = CheckState.Indeterminate;
            }

            if (_enableMultipleChoices == false)
            {
                lstOptions.ItemCheck -= lstOptions_ItemCheck;

                for (int ix = 0; ix < lstOptions.Items.Count; ++ix)
                    if (ix != e.Index) lstOptions.SetItemChecked(ix, false);

                lstOptions.ItemCheck += lstOptions_ItemCheck;
            }
        }
    }
}
