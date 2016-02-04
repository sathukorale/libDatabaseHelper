using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DatabaseEntityFormCreator
{
    public partial class frmMatchingTypes : Form
    {
        private static frmMatchingTypes _presentedForm;
        private static Type[] _types;

        struct DbeType
        {
            private readonly Type _type;

            public DbeType(Type type)
            {
                _type = type;
            }

            public new Type GetType()
            {
                return _type;
            }

            public override string ToString()
            {
                return _type.Name;
            }
        }

        public frmMatchingTypes()
        {
            InitializeComponent();
        }

        private void LoadTypes(Type[] types)
        {
            _types = types;
            chklstMatchingTypes.Items.Clear();
            foreach (var type in types.Select(i => new DbeType(i)))
            {
                chklstMatchingTypes.Items.Add(type, true);
            }
        }

        public static Type[] ShowWindow(Type[] types)
        {
            if (_presentedForm == null || _presentedForm.IsDisposed)
                _presentedForm = new frmMatchingTypes();
            _presentedForm.LoadTypes(types);
            _presentedForm.ShowDialog();
            return _types;
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            _types = chklstMatchingTypes.CheckedItems.OfType<DbeType>().Select(i => i.GetType()).ToArray();
            Hide();
        }
    }
}
