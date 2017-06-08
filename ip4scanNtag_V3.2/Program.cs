using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ip4scanNtag
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            try
            {
                Application.Run(new mainForm());
            }
            catch (SystemException sx)
            {
                MessageBox.Show("Exception: \n" + sx.Message, "Uuups");
            }
        }
    }
}