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
    public partial class frmCreatorMain : Form
    {
        public frmCreatorMain()
        {
            InitializeComponent();
        }

        private void btnCreateForms_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Please Select the .NET Library Containing, the Classes Extending DatabaseEntity";
                ofd.Filter = "Compiled DLL Files|*.dll";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Creator.CreateForm(ofd.FileName, Creator.EntityControlType.DatabaseEntityForm);
                }
            }
        }

        private void btnCreateUserControls_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Please Select the .NET Library Containing, the Classes Extending DatabaseEntity";
                ofd.Filter = "Compiled DLL Files|*.dll";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Creator.CreateForm(ofd.FileName, Creator.EntityControlType.DatabaseEntityUserControl);
                }
            }
        }
    }
}
