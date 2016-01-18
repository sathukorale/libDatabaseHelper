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

        private void button1_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Compiled DLL Files|*.dll";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    Creator.CreateForm(ofd.FileName);
                }
            }
        }
    }
}
