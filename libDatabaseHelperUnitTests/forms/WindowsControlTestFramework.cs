using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using libDatabaseHelper.classes.generic;

namespace libDatabaseHelperUnitTests.forms
{
    class WindowControlContext
    {
        public static Control GetElementByName(Control parent_control, string name)
        {
            var controls = FormUtils.GetAllElements(parent_control);
            controls = controls.Where(i => i.Name != null && i.Name == name).ToList();
            if (controls.Any())
            {
                return controls.First();
            }
            return null;
        }

        public static List<Control> GetElementByTagType(Control parent_control, Type type)
        {
            var controls = FormUtils.GetAllElements(parent_control);
            controls = controls.Where(i => i.GetType() == type).ToList();
            return controls;
        }
    }

    class WindowsControlTestFramework
    {
        private static Control _root_control;

        public static void SetRoot(Control root_control)
        {
            _root_control = root_control;
        }

        public static void SetControlValue(string control_name, Object value)
        {
            SetControlAttribute(control_name, "Value", value);
            SetControlAttribute(control_name, "Text", value);
        }

        public static void SetControlAttribute(string control_name, string attribute_name, Object value)
        {
            if (_root_control == null)
            {
                throw new Exception("The \"Root Control\" element is not set !");
            }

            var element = WindowControlContext.GetElementByName(_root_control, control_name);
            if (element != null)
            {
                FieldInfo[] fields = element.GetType().GetFields(BindingFlags.Public).Where(i => i.Name == attribute_name).ToArray();
                if (fields.Any())
                {
                    //(FieldInfo(fields[0])).SetValue(element, value);
                }
            }
        }

        public static Object GetControlValue(string control_name, Object value)
        {
            var value_read = GetControlAttribute(control_name, "Text", value);
            if (value_read == null || value_read.ToString().Trim() == "")
            {
                return null;
            }
            return value_read;
        }

        public static Object GetControlAttribute(string control_name, string attribute_name, Object value)
        {
            if (_root_control == null)
            {
                throw new Exception("The \"Root Control\" element is not set !");
            }

            var element = WindowControlContext.GetElementByName(_root_control, control_name);
            if (element != null)
            {
                FieldInfo[] fields = element.GetType().GetFields(BindingFlags.Public).Where(i => i.Name == attribute_name).ToArray();
                if (fields.Any())
                {
                    return null;// (FieldInfo(fields[0])).GetValue(element);
                }
            }

            return null;
        }
    }
}
