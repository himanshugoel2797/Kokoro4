using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kokoro4.ProjectManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void newPrjtBtn_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                //show project creation dialog 
            }
        }

        private void openPrjtBtn_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                //load project and add it to content set
            }
        }
    }
}
