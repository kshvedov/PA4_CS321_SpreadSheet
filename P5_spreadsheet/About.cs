/*
 * Name: Konstantin Shvedov
 * Date: 26/11/2018
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace P5_spreadsheet
{
    public partial class About : Form
    {
        public About(double ver)
        {
            InitializeComponent();
            label2.Text = "Version " + ver.ToString("F");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                linkLabel1.LinkVisited = true;
                System.Diagnostics.Process.Start("https://creativecommons.org/licenses/by/4.0/");
            }
            catch (Exception newEx)
            {
                MessageBox.Show("Unable to open link");
            }
        }
    }
}
