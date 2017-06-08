using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ip4scanNtag
{
    public partial class PasswordDlg : Form
    {
        Microsoft.WindowsCE.Forms.InputPanel ip = new Microsoft.WindowsCE.Forms.InputPanel();
        public PasswordDlg()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == "cr52401")
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
            Microsoft.WindowsCE.Forms.InputPanel ip = new Microsoft.WindowsCE.Forms.InputPanel();
            ip.Enabled = false;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            ip.Enabled = false;
            this.Close();
        }

        private void mnuOK_Click(object sender, EventArgs e)
        {
            btnOK_Click(sender, e);
        }

        private void mnuCancel_Click(object sender, EventArgs e)
        {
            btnCancel_Click(sender, e);
        }

        private void PasswordDlg_Load(object sender, EventArgs e)
        {
            ip.Enabled = true;
        }
    }
}