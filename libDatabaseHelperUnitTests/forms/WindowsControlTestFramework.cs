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
        public static Control GetElementByName(Control parentControl, string name)
        {
            var controls = FormUtils.GetAllElements(parentControl);
            controls = controls.Where(i => i.Name != null && i.Name == name).ToList();
            if (controls.Any())
            {
                return controls.First();
            }
            return null;
        }

        public static List<Control> GetElementByTagType(Control parentControl, Type type)
        {
            var controls = FormUtils.GetAllElements(parentControl);
            controls = controls.Where(i => i.GetType() == type).ToList();
            return controls;
        }
    }

    class WindowsControlTestFramework
    {
        private static Control _rootControl;

        public static void SetRoot(Control rootControl)
        {
            _rootControl = rootControl;
        }

        public static void SetControlValue(string controlName, Object value)
        {
            SetControlAttribute(controlName, "Value", value);
            SetControlAttribute(controlName, "Text", value);
        }

        public static void SetControlAttribute(string controlName, string attributeName, Object value)
        {
            if (_rootControl == null)
            {
                throw new Exception("The \"Root Control\" element is not set !");
            }

            var element = WindowControlContext.GetElementByName(_rootControl, controlName);
            if (element != null)
            {
                FieldInfo[] fields = element.GetType().GetFields(BindingFlags.Public).Where(i => i.Name == attributeName).ToArray();
                if (fields.Any())
                {
                    //(FieldInfo(fields[0])).SetValue(element, value);
                }
            }
        }

        public static Object GetControlValue(string controlName, Object value)
        {
            var valueRead = GetControlAttribute(controlName, "Text", value);
            if (valueRead == null || valueRead.ToString().Trim() == "")
            {
                return null;
            }
            return valueRead;
        }

        public static Object GetControlAttribute(string controlName, string attributeName, Object value)
        {
            if (_rootControl == null)
            {
                throw new Exception("The \"Root Control\" element is not set !");
            }

            var element = WindowControlContext.GetElementByName(_rootControl, controlName);
            if (element != null)
            {
                FieldInfo[] fields = element.GetType().GetFields(BindingFlags.Public).Where(i => i.Name == attributeName).ToArray();
                if (fields.Any())
                {
                    return null;// (FieldInfo(fields[0])).GetValue(element);
                }
            }

            return null;
        }
    }
}
