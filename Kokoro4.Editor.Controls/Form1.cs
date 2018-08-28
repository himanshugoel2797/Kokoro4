using Kokoro.Editor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kokoro4.Editor.Controls
{
    public partial class Form1 : Form
    {
        CommunicationManager communicationManager;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            communicationManager = new CommunicationManager();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            communicationManager.ReadTest();
        }
    }
}
