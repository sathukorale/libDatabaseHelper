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
    public partial class NamedComboBox : SearchFilterControl
    {
        public NamedComboBox() : base() { }

        public NamedComboBox(Type classType, string fieldName) : base(classType, fieldName)
        {
            InitializeComponent();

            SetLabelControl(lblName);
            SetControlRemoveButton(btnRemoveControl);

            if (GenericFieldTools.IsTypeBool(FieldInfo == null ? null : FieldInfo.FieldType))
            {
                cmbItemList.Items.Clear();
                cmbItemList.Items.AddRange(new object[] { "True", "False" });
            }
        }

        public override Selector GetSearchFilter()
        {
            if (GenericFieldTools.IsTypeBool(FieldInfo == null ? null : FieldInfo.FieldType))
            {
                return new Selector(FieldName, cmbItemList.SelectedIndex == 0 ? true : false, Selector.Operator.Like);
            }
            else
            {
                return new Selector(FieldName, cmbItemList.SelectedItem, Selector.Operator.Like);
            }
        }
    }
}
