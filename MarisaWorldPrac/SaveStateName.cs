using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperMarisaWorldPrac
{
    public partial class SaveStateName : Form
    {
        public SaveStateName()
        {
            InitializeComponent();
            CenterToScreen();
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
        }
        public string name
        {
            get { return textName.Text; }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textName.Text.Length > 0)
            {
                textName.Text = textName.Text.Replace("|", "");
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("No value entered. Try again.",
                                "Nice name",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }
    }
}
