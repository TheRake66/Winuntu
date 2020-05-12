using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Winuntu
{
    //=====================================================================
    public partial class MenuInfos : Form
    {
        //=====================================================================
        public MenuInfos()
        {
            //-----------------------------------------------
            InitializeComponent();
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void MenuInfos_Load(object sender, EventArgs e)
        {
            //-----------------------------------------------
            label5.Text = "Version " + ((Menu)Owner).VERSION + " (" + ((Menu)Owner).ARCH + ")";
            label6.Text = ((Menu)Owner).COPYRIGHT;
            //-----------------------------------------------
        }
        //=====================================================================



        //=====================================================================
        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //-----------------------------------------------
            try
            {
                Process.Start("explorer.exe", linkLabel1.Text);
            }
            catch { }
            //-----------------------------------------------
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            //-----------------------------------------------
            this.Close();
            //-----------------------------------------------
        }
        //=====================================================================
    }
    //=====================================================================
}
