using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using libDatabaseHelper.forms;
using libDatabaseHelper.classes.generic;
using libDatabaseHelperUnitTests.sqlce;

namespace libDatabaseHelperUnitTests.forms.sqlce
{
    public partial class frmSampleTable1 : DatabaseEntityForm
    {
        public frmSampleTable1()
        {
            InitializeComponent();

            SetOkButton(btnOK);
            SetResetButton(btnReset);
            SetCancelButton(btnCancel);
			
			txtColumn2.Tag = new TableColumnField(true, typeof(SampleTable1), "Column2");

			_type=typeof(SampleTable1);
        }

        public void SetValues(string column2_value)
        {
            txtColumn2.Text = column2_value;
        }
    }
}