using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using libDatabaseHelper.classes.generic;

namespace libDatabaseHelper.forms.controls
{
    public partial class NamedDateTimePicker : SearchFilterControl
    {
        public NamedDateTimePicker() : base() { }

        public NamedDateTimePicker(Type classType, string fieldName) : base(classType, fieldName)
        {
            InitializeComponent();

            SetLabelControl(lblName);
            SetControlRemoveButton(btnRemoveControl);
        }

        public override Selector GetSearchFilter()
        {
            return new Selector(FieldName, dtpCalendar.Value, Selector.Operator.Like);
        }
    }
}
