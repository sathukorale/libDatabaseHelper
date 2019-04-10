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
using libDatabaseHelperUnitTests.mysql;

namespace libDatabaseHelperUnitTests.forms.mysql
{
    public partial class FrmSampleTable1 : DatabaseEntityForm
    {
        public FrmSampleTable1()
        {
            InitializeComponent();

            SetOkButton(btnOK);
            SetResetButton(btnReset);
            SetCancelButton(btnCancel);
			
			txtColumn2.Tag = new TableColumnField(true, typeof(SampleTable1), "Column2");

			SetEntityType(typeof(SampleTable1));
        }

        public void SetValues(string column2Value)
        {
            txtColumn2.Text = column2Value;
        }
    }
}