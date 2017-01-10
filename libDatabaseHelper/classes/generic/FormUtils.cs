using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace libDatabaseHelper.classes.generic
{
    public class FormUtils
    {
        struct ControlDetails
        {
            public Keys[] key;
            public Func<Keys, bool> method;

            public ControlDetails(Keys[] keyExpected, Func<Keys, bool> methodToCall)
            {
                key = keyExpected;
                method = methodToCall;
            }
        }

        private const int EM_SETCUEBANNER = 0x1501;

        private static Dictionary<Control, ControlDetails> _sensibleKeys = new Dictionary<Control, ControlDetails>();
        private static Dictionary<Control, List<Control>> _all_elements = new Dictionary<Control, List<Control>>();
        private static Dictionary<IntPtr, ToolTip> _createdToolTips = new Dictionary<IntPtr, ToolTip>();
        private static Dictionary<TextBox, string> _placeholderList = new Dictionary<TextBox, string>();

        public static void ClearForm(Control parentControl)
        {
            if (!parentControl.HasChildren)
                return;

            foreach (Control control in parentControl.Controls)
            {
                if (control is TextBox)
                {
                    var txt = control as TextBox;
                    txt.Clear();
                    txt.ReadOnly = false;
                }
                else if (control is ComboBox)
                {
                    var cmb = (control as ComboBox);
                    cmb.SelectedItem = null;
                    if (cmb.Items.Count > 0)
                    {
                        cmb.SelectedIndex = 0;
                        cmb.SelectedItem = cmb.Items[0];
                        try
                        {
                            //cmb.SelectionLength = 0;
                        }
                        catch (Exception) { }
                    }
                }
                else if (control is CheckBox)
                    (control as CheckBox).Checked = false;
                else if (control is DateTimePicker)
                    (control as DateTimePicker).Value = DateTime.Now;
                else if (control.HasChildren)
                    ClearForm(control);
                control.Enabled = true;
            }
        }

        private static List<Control> _GetAllElements(Control parentControl)
        {
            var list = new List<Control>();
            if (parentControl.HasChildren)
            {
                list.AddRange(_GetAllElements(parentControl));
            }
            list.Add(parentControl);
            return list;
        }

        public static List<Control> GetAllElements(Control parentControl)
        {
            if (_all_elements.ContainsKey(parentControl))
            {
                return _all_elements[parentControl];
            }
            else
            {
                var list = _GetAllElements(parentControl);
                _all_elements[parentControl] = list;
                return list;
            }
        }

        public static void AddToolTip(Control control, string toolTip)
        {
            AddToolTip(control, toolTip, ToolTipIcon.None);
        }

        public static void AddToolTip(Control control, string toolTip, ToolTipIcon icon)
        {
            var form = GetParentForm(control);
            if (form == null)
            {
                return;
            }

            ToolTip toolTipElement = null;
            if (_createdToolTips.ContainsKey(form.Handle))
            {
                toolTipElement = _createdToolTips[form.Handle];
            }
            else
            {
                toolTipElement = new ToolTip();
                toolTipElement.Active = true;
                toolTipElement.AutomaticDelay = 0;
                toolTipElement.AutoPopDelay = 10000;
                toolTipElement.IsBalloon = false;
                toolTipElement.ShowAlways = true;
                toolTipElement.UseAnimation = false;
                toolTipElement.ReshowDelay = 0;
                toolTipElement.ToolTipIcon = icon;
            }
            toolTipElement.SetToolTip(control, toolTip);
        }

        private static Form GetParentForm(Control control)
        {
            while (control.Parent != null)
            {
                control = control.Parent;
            }
            return control as Form;
        }

        public static Control GetFirstParentOfType<T>(Control control)
        {
            return GetFirstParentOfType(typeof(T), control);
        }

        public static Control GetFirstParentOfType(Type type, Control control)
        {
            if (control == null) return null;

            while (true)
            {
                control = control.Parent;
                if (control != null)
                {
                    if (control.GetType() == type)
                    {
                        return control;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public static void MakeControlAndSubControlsSensitiveToKey(Control control, Keys[] keys, Func<Keys, bool> method)
        {
            control.KeyUp += (object sender, KeyEventArgs e) =>
            {
                Control control_sent = (Control)sender;
                if (_sensibleKeys.ContainsKey(control))
                {
                    foreach (var key in _sensibleKeys[control].key)
                    {
                        if (key == e.KeyCode)
                        {
                            _sensibleKeys[control_sent].method(e.KeyCode);
                        }
                    }
                }
            };
            _sensibleKeys.Add(control, new ControlDetails(keys, method));

            foreach (Control child_control in control.Controls)
            {
                MakeControlAndSubControlsSensitiveToKey(child_control, keys, method);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)]string lParam);

        public static void AddPlaceHolder(TextBox txt, string text)
        {
            try
            {
                if (txt.Multiline)
                {
                    // TODO : 
                }
                else
                {
                    SendMessage(txt.Handle, EM_SETCUEBANNER, 0, text);
                }
            }
            catch { }
        }
    }
}
