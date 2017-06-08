using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ip4scanNtag;

namespace TestVWtag
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        public string sTestBarCode = "1J UN 04-997-7473 123456789";
        public string sBoxID = "4000000";
        private static ip4scanNtag.VWGTLTAG vwtag;
        
        private void btnStart_Click(object sender, EventArgs e)
        {
            string s;
            s = txtBarCode.Text.Replace(" ", "");
            s = s.Replace("-", "");
            txtBarCodeClean.Text = s;

            if (!VWGTLTAG.CheckBarCode(s))
            {
                txtBarCode.BackColor = Color.Red;
                return;
            }
            else
                txtBarCode.BackColor = Color.LightGreen;

            txtBoxFilter.Text = txtBarCodeClean.Text.Substring(0, 4);
            txtSupplier.Text = txtBarCodeClean.Text.Substring(4, 9);
            txtItemNumber.Text = txtBarCodeClean.Text.Substring(13, 9);
            txtBoxID.Text = "100";
            VWGTLTAG tag1 = new VWGTLTAG(txtBoxFilter.Text, txtSupplier.Text, txtItemNumber.Text, "1");
            txtBarCodeHex.Text = tag1.GetHex();//OK

            VWGTLTAG tag2 = new VWGTLTAG(txtBarCodeHex.Text);
            if (!tag1.GetHex().Equals(tag2.GetHex()))
                System.Diagnostics.Debugger.Break();
        }

        private void btnEncodeFilter_Click(object sender, EventArgs e)
        {
            vwtag = new ip4scanNtag.VWGTLTAG(   txtBoxFilter.Text, 
                                                txtSupplier.Text, 
                                                txtItemNumber.Text ,
                                                txtBoxID.Text );
            txtTagHex.Text = vwtag.GetHex();

            txtTagRead.Text = vwtag.ToString();
        }

        private void btnDecodeTagFromHex_Click(object sender, EventArgs e)
        {
            if (!VWGTLTAG.IsValidTag(txtTagHex.Text))
            {
                txtTagHex.BackColor = Color.Red;
                return;
            }
            else
                txtTagHex.BackColor = Color.LightGreen;
            ip4scanNtag.VWGTLTAG vwtag1 = new ip4scanNtag.VWGTLTAG(txtTagHex.Text);
            //read values
            txtBoxFilter.Text = vwtag1.sFilter;
            txtSupplier.Text = vwtag1.iSupplierID.ToString();
            txtItemNumber.Text = vwtag1.iItemNumber.ToString();
            txtBoxID.Text = vwtag1.BoxID.ToString();

        }
    }

}