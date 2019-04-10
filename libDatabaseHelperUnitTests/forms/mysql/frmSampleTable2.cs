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
    public partial class FrmSampleTable2 : DatabaseEntityForm
    {
        public FrmSampleTable2()
        {
            InitializeComponent();

            SetOkButton(btnOK);
            SetResetButton(btnReset);
            SetCancelButton(btnCancel);
			
			txtColumn2.Tag = new TableColumnField(true, typeof(SampleTable2), "Column2");
			cmbColumn3.Tag = new TableColumnField(true, typeof(SampleTable2), "Column3");

            SetEntityType(typeof(SampleTable2));
			LoadColumns();
        }
		
		#region "Autogenerated Data Loaders"		

		private void LoadColumns()
        {
            try
            {
                var items = GenericDatabaseManager.GetDatabaseManager(DatabaseType.SqlCE).Select<SampleTable1>(null);
                cmbColumn3.Items.Clear();
                cmbColumn3.Items.AddRange(items);
            }
            catch { }
        }
		
		#endregion
    }
}