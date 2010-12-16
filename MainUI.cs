using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using Extron;

namespace Kramer
{
    public partial class MainUI : Form
    {
        Extron.ExtronSwitcher switcher = null;

        public MainUI()
        {
            InitializeComponent();

            switcher = new ExtronSwitcher("COM4", 9600, Parity.None, 8, StopBits.One);
            if (switcher.Open()) this.Text = "Ready";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switcher.Synchronize();
            textBox1.Text =  switcher.CurrentTies;
            
        }

        private void MainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            switcher.Close();
        }
    }
}