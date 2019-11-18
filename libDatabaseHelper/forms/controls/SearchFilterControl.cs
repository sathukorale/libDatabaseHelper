using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using libDatabaseHelper.classes.generic;

namespace libDatabaseHelper.forms.controls
{
    public class SearchFilterControl : UserControl
    {
        public readonly Type ClassType;
        public string FieldName;

        private Label _btnLabel;
        private Button _btnRemove;

        protected FieldInfo  FieldInfo;
        protected TableColumn FieldAttributes;

        public delegate void RemoveControlEventHandler(SearchFilterControl sender);
        public event RemoveControlEventHandler OnRemoveControl;

        public SearchFilterControl() { }

        public SearchFilterControl(Type classType, string fieldName)
        {
            ValidateFieldInfo(classType, fieldName);

            ClassType = classType;
            FieldName = fieldName;
        }

        protected void ValidateFieldInfo(Type classType, string fieldName)
        {
            var classInstance = GenericDatabaseEntity.GetNonDisposableReferenceObject(classType);
            if (classInstance == null)
            {
                throw new Exception("The class type '" + classType.FullName + "' should be derived from 'GenericDatabaseEntity' for it to be used with 'SearchFilterControl'");
            }

            var fieldAttributes = classInstance.GetFieldAttributes(fieldName);
            if (fieldAttributes == null)
            {
                throw new Exception("The field '" + fieldName + "', could not be found within the class of type '" + classType.FullName + "'");
            }

            FieldInfo = classInstance.GetFieldInfo(fieldName);
            FieldAttributes = fieldAttributes;
        }

        protected void SetLabelControl(Label control)
        {
            _btnLabel = control;
            _btnLabel.Text = FieldAttributes.GridDisplayName;
        }

        protected void SetControlRemoveButton(Button control)
        {
            _btnRemove = control;
            _btnRemove.Click += BtnRemove_OnClick;
        }

        private void BtnRemove_OnClick(object sender, EventArgs eventArgs)
        {
            if (OnRemoveControl != null) OnRemoveControl.Invoke(this);
        }

        public virtual Selector GetSearchFilter()
        {
            throw new Exception("This method should be overridden and implemented on the corresponding imeplementation");
        }
    }
}
