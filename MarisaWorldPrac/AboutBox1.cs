using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
            textDescription.Text = "Tool geared towards making dirty p easier than ever.\n\n" +

                                   "If the tool isn't unlocking any elements when you are in a stage, click on the 'If broken, click here to fix' button, you will be prompted to go onto the world 0 map, then click OK. " +
                                   "If the fix was succesful then nice, if not then A.sorry...\n\n" +

                                   "Hotkeys can be used to switch between power-ups, save coordinates, load them and set 'Stars', 'Time' & 'Score' to the specified values.\n" +
                                   "Note that the selected hotkeys will become unusable elsewhere while the tool is open.\n\n" +
                                   
                                   "There are 3 main sections in the tool:\n\n" +
                                   
                                   "'Save states' section shows Marisa's current coordinates, allows storing values from using the appropriate hotkey or clicking the 'Store' button or selecting a save state from the dropdown list.\n" +
                                   "The 'Save' button will save Marisa's coordinates for the current screen, and it can also save the 'Stars', 'Time' & 'Score' values if the checkbox is checked." +
                                   " Once clicked, the save state will be added to the dropdown list and can be used whenever Marisa is on that same screen.\n" +
                                   "To load values after either clicking 'Store' or creating a save state with 'Save', simply click on 'Load' or use the appropriate hotkey.\n" +
                                   "This section also allows the input of a custom set of coordinates (game crashes if the value is too big).\n\n" +
                                   
                                   "'Freeze values' section will freeze values upon checking one of the checkboxes.\n" +
                                   "'Stars', 'Time' and 'Score' will be frozen at the value specified in their respective textboxes below.\n" +
                                   "'Lives' and 'Power-up' will be frozen at whatever their values were upon checking their boxes while 'P-speed+fly' will always be set to 120 and 200 respectively for the memes.\n\n" +
                                   
                                   "'P-speed' section displays Marisa's P-speed meter when she has her broom. From what I found, this value goes from 0 to 120 in increments of 2.\n" +
                                   "This will also display the meter value Marisa had as soon as she jumps to get an idea of how close/far Marisa was from reaching P-speed.\n\n" +
                                   
                                   "Only version 1.05 is supported.";
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
