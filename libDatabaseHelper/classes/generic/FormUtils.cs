﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace libDatabaseHelper.classes.generic
{
    class FormUtils
    {
        struct ControlDetails
        {
            public Keys[] key;
            public Func<Keys, int> method;

            public ControlDetails(Keys[] keyExpected, Func<Keys, int> methodToCall)
            {
                key = keyExpected;
                method = methodToCall;
            }
        }

        private static ToolTip _universalToolTip = null;
        private static Dictionary<int, ControlDetails> _sensibleKeys = new Dictionary<int, ControlDetails>();
        private static Dictionary<Control, List<Control>> _all_elements = new Dictionary<Control, List<Control>>();

        public static void ClearForm(Control parentControl)
        {
            if (!parentControl.HasChildren)
                return;

            foreach (Control control in parentControl.Controls)
            {
                if (control is TextBox)
                    (control as TextBox).Clear();
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

        public static void AddToolTip(Control control, string toolTip, ToolTipIcon icon)
        {
            if (_universalToolTip == null)
            {
                _universalToolTip = new ToolTip();
                _universalToolTip.Active = true;
                _universalToolTip.AutomaticDelay = 0;
                _universalToolTip.AutoPopDelay = 0;
                _universalToolTip.IsBalloon = false;
                _universalToolTip.ShowAlways = true;
                _universalToolTip.UseAnimation = true;
                _universalToolTip.ReshowDelay = 0;
                _universalToolTip.ToolTipIcon = icon;
            }
            _universalToolTip.SetToolTip(control, toolTip);
        }

        public static void MakeControlAndSubControlsSensitiveToKey(Control control, Keys[] keys, Func<Keys, int> method)
        {
            control.KeyDown += (object sender, KeyEventArgs e) =>
            {
                Control control_sent = (Control)sender;
                if (_sensibleKeys.ContainsKey(control.GetHashCode()))
                {
                    foreach (var key in _sensibleKeys[control.GetHashCode()].key)
                    {
                        if (key == e.KeyCode)
                        {
                            _sensibleKeys[control_sent.GetHashCode()].method(e.KeyCode);
                        }
                    }
                }
            };
            _sensibleKeys.Add(control.GetHashCode(), new ControlDetails(keys, method));

            foreach (Control child_control in control.Controls)
            {
                MakeControlAndSubControlsSensitiveToKey(child_control, keys, method);
            }
        }
    }
}