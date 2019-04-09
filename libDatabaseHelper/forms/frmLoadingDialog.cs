using System;
using System.Windows.Forms;
using System.Threading;

namespace libDatabaseHelper.forms
{
    public partial class frmLoadingDialog : Form
    {
        private static bool _cancel;
        private static int _isBusy;
        private static string _message = "Please wait while the list is being populated";
        private static frmLoadingDialog _presentedDialog;

        private static Func<object, object> _methodToRun;
        private static object _methodParameters;

        private static object _current_execution_return_detail;

        private frmLoadingDialog()
        {
            InitializeComponent();
            prgLoadingProgress.Maximum = 100;

            Shown += OnShown;
        }

        private void OnShown(object sender, EventArgs eventArgs)
        {
            var t = new Thread(delegate (object o)
            {
                _current_execution_return_detail = _methodToRun(o);
                Interlocked.Exchange(ref _isBusy, 0);

                try
                {
                    HideWindow();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            t.Start(_methodParameters);
        }

        public static void ShowWindow()
        {
            ShowWindow("Please wait while the list is being populated");
        }

        public static void ShowWindow(string message, IWin32Window owner = null)
        {
            if (_presentedDialog == null || _presentedDialog.IsDisposed)
                _presentedDialog = new frmLoadingDialog();
            _message = message;
            _cancel = false;
            _presentedDialog.SetUp(false);
            if (owner == null)
            {
                _presentedDialog.Show();
            }
            else
            {
                _presentedDialog.ShowDialog(owner);
            }
        }

        public static object ShowWindow(Func<object, object> method, object param, string message, IWin32Window owner = null)
        {
            return ShowWindow(method, param, message, false, owner);
        }

        public static object ShowWindow(Func<object, object> method, object param, string message, bool showProgress, IWin32Window owner = null)
        {
            if (Thread.VolatileRead(ref _isBusy) == 1)
                return null;

            if (_presentedDialog == null || _presentedDialog.IsDisposed)
                _presentedDialog = new frmLoadingDialog();

            Interlocked.Exchange(ref _isBusy, 1);
            _current_execution_return_detail = null;

            _methodToRun = method;
            _methodParameters = param;
            _message = message;
            _cancel = false;
            _presentedDialog.SetUp(showProgress);

            _presentedDialog.Hide();
            _presentedDialog.ShowDialog(owner);

            return _current_execution_return_detail;
        }

        private void SetUp(bool showProgress)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>(value =>
                {
                    lblMessage.Text = _message;
                    prgLoadingProgress.Value = 0;
                    prgLoadingProgress.Style = ProgressBarStyle.Marquee;
                    Application.DoEvents();
                }), showProgress);
            }
            else
            {
                lblMessage.Text = _message;
                prgLoadingProgress.Value = 0;
                prgLoadingProgress.Style = ProgressBarStyle.Marquee;
                Application.DoEvents();
            }
        }

        private void UpdateProgress(int value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>((objValue) => {
                    prgLoadingProgress.Style = ((int)objValue == 0) ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                    prgLoadingProgress.Value = (int)objValue;
                    Application.DoEvents();
                }), value);
            }
            else
            {
                prgLoadingProgress.Style = (value == 0) ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                prgLoadingProgress.Value = value;
                Application.DoEvents();
            }

        }

        private void UpdatText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            if (IsHandleCreated)
            {
                BeginInvoke(new Action<object>((objValue) =>
                {
                    lblMessage.Text = (string)objValue;
                    Application.DoEvents();
                }), text);
            }
        }

        public static void UpdateProgressPercentage(int value)
        { 
            if (_presentedDialog != null && _presentedDialog.IsDisposed == false)
            {
                if (_presentedDialog.InvokeRequired)
                {
                    _presentedDialog.BeginInvoke(new Action<object>(objectPassed =>
                    {
                        _presentedDialog.UpdateProgress((int)objectPassed);
                        Application.DoEvents();
                    }), value);
                }
                else
                {
                    _presentedDialog.UpdateProgress(value);
                    Application.DoEvents();
                }
            }
        }

        public static void UpdateProgressText(string text)
        {
            if (_presentedDialog != null && _presentedDialog.IsDisposed == false)
            {
                if (_presentedDialog.InvokeRequired)
                {
                    _presentedDialog.BeginInvoke(new Action<object>(objectPassed => _presentedDialog.UpdatText((string)objectPassed)), text);
                    Application.DoEvents();
                }
                else
                {
                    _presentedDialog.UpdatText(text);
                    Application.DoEvents();
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

            if (_presentedDialog.InvokeRequired)
            {
                try
                {
                    _presentedDialog.BeginInvoke(new Action<object>(objectPassed => _presentedDialog.Close()), new object[] { null });
                }
                catch { }
            }
            else
            {
                _presentedDialog.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cancel = true;
            Close();
        }
    }
}
