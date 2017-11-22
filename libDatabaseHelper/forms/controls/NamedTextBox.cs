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
    public partial class NamedTextBox : SearchFilterControl
    {
        public NamedTextBox() : base() { }

        public NamedTextBox(Type classType, string fieldName) : base(classType, fieldName)
        {
            InitializeComponent();

            SetLabelControl(lblName);
            SetControlRemoveButton(btnRemoveControl);

            FormUtils.AddToolTip(btnRemoveControl, "Remove Filter");
        }

        public override Selector GetSearchFilter()
        {
            return new Selector(FieldName, txtSearchTerm.Text, Selector.Operator.Like);
        }
    }
}
