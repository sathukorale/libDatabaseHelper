using System;
using System.Windows.Forms;
using System.Threading;

namespace libDatabaseHelper.forms
{
    public partial class frmLoadingDialog : Form
    {
        private static bool _cancel;
        private static bool _is_busy;
        private static string _message = "Please wait while the list is being populated";
        private static frmLoadingDialog _presentedDialog;

        private static object _current_execution_return_detail;

        public frmLoadingDialog()
        {
            InitializeComponent();
        }

        public static void ShowWindow()
        {
            ShowWindow("Please wait while the list is being populated");
        }

        public static void ShowWindow(string message)
        {
            if (_presentedDialog == null || _presentedDialog.IsDisposed)
                _presentedDialog = new frmLoadingDialog();
            _message = message;
            _cancel = false;
            _presentedDialog.SetUp(false);
            _presentedDialog.Show();
        }

        public static object ShowWindow(Func<object, object> method, object param, string message)
        {
            return ShowWindow(method, param, message, false);
        }

        public static object ShowWindow(Func<object, object> method, object param, string message, bool showProgress)
        {
            if (_is_busy)
                return null;

            if (_presentedDialog == null || _presentedDialog.IsDisposed)
                _presentedDialog = new frmLoadingDialog();

            _is_busy = true;
            _current_execution_return_detail = null;
            var t = new Thread(delegate(object o)
            {
                _current_execution_return_detail = method(o);
                _is_busy = false;
                try
                {
                    _presentedDialog.Invoke(new Action<object>(objectPassed => HideWindow()), new object[]{null});
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            _message = message;
            _cancel = false;
            _presentedDialog.SetUp(showProgress);
            t.SetApartmentState(ApartmentState.STA);
            t.Start(param);
            if (_is_busy)
            {
                _presentedDialog.ShowDialog();
            }
            return _current_execution_return_detail;
        }

        private void SetUp(bool showProgress)
        {
            lblMessage.Text = _message;
            Height = showProgress ? 133 : 109;
            prgLoadingProgress.Visible = showProgress;
            prgLoadingProgress.Value = 0;
        }

        private void UpdateProgress(int value)
        {
            if (value == 0)
            {
                prgLoadingProgress.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                prgLoadingProgress.Style = ProgressBarStyle.Continuous;
            }

            prgLoadingProgress.Value = value;
        }

        public static void UpdateProgressPercentage(int value)
        { 
            if (_presentedDialog != null && _presentedDialog.IsDisposed == false)
            {
                if (_presentedDialog.InvokeRequired)
                {
                    _presentedDialog.BeginInvoke(new Action<object>(objectPassed => _presentedDialog.UpdateProgress((int)objectPassed)), value);
                }
                else
                {
                    _presentedDialog.UpdateProgress(value);
                }
            }
        }

        public static bool GetStatus()
        {
            Application.DoEvents();
            return _cancel;
        }

        public static void HideWindow()
        {
            if (_presentedDialog == null || _presentedDialog.IsDisposed)
                return;
            _presentedDialog.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cancel = true;
            Close();
        }
    }
}
