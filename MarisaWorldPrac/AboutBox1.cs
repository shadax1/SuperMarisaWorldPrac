using System;
using System.Windows.Forms;

namespace SuperMarisaWorldPrac
{
    partial class AboutBox1 : Form
    {
        bool yay = false;

        public AboutBox1()
        {
            InitializeComponent();
            CenterToScreen();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void logoPictureBox_Click(object sender, EventArgs e)
        {
            if(!yay)
            {
                logoPictureBox.Image = Properties.Resources.Koakuma_2;
                yay = true;
            }
            else
            {
                logoPictureBox.Image = Properties.Resources.Koakuma_1;
                yay = false;
            }
        }
    }
}
