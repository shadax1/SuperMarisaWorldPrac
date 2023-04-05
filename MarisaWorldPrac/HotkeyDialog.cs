﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SuperMarisaWorldPrac
{
    public partial class HotkeyDialog : Form
    {
        static string hotkeyVersion = "v1.5";
        static string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string configpath = appdata + @"\SMWPrac\";
        string hotkeyfilename = "hotkey.cfg";

        public HotkeyDialog()
        {
            InitializeComponent();
            CenterToScreen();
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;

            foreach (CustomKeys k in Enum.GetValues(typeof(CustomKeys)))
            {
                comboHotkey1.Items.Add(k);
                comboHotkey2.Items.Add(k);
                comboHotkey3.Items.Add(k);
                comboHotkey4.Items.Add(k);
                comboHotkey5.Items.Add(k);
                comboHotkey6.Items.Add(k);
            }

            foreach (ModifierKeys k in Enum.GetValues(typeof(ModifierKeys)))
            {
                comboModifier1.Items.Add(k);
                comboModifier2.Items.Add(k);
                comboModifier3.Items.Add(k);
                comboModifier4.Items.Add(k);
                comboModifier5.Items.Add(k);
                comboModifier6.Items.Add(k);
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            using (StreamWriter sw = File.CreateText(configpath + hotkeyfilename)) //saving hotkeys
            {
                sw.WriteLine(hotkeyVersion); //stores the value of the combobox inside the .cfg
                //store combobox values by parsing all components inside the form in the TabIndex order
                foreach (Control c in Controls.Cast<Control>().OrderBy(c => c.TabIndex))
                    if (c is ComboBox) //if they are comboboxes
                        sw.WriteLine(c.Text); //stores the value of the combobox inside the .cfg
            }
            Close();
        }

        private void HotkeyDialog_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(configpath))
                Directory.CreateDirectory(configpath);

            if (File.Exists(configpath + hotkeyfilename)) //checks if config.cfg exists
            {
                using (StreamReader sr = File.OpenText(configpath + hotkeyfilename))
                {
                    sr.ReadLine(); //skip line with version
                    //loads comboboxes with values by parsing all components inside the form in the TabIndex order
                    foreach (Control c in Controls.Cast<Control>().OrderBy(c => c.TabIndex)) 
                        if (c is ComboBox) //if they are comboboxes
                            c.Text = sr.ReadLine(); //reads the value from the .cfg and puts it into the combobox
                }
            }
        }

        private void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            byte i = 0;
            HashSet<string> hs = new HashSet<string>();
            string[] a =
            {comboModifier1.SelectedItem.ToString() + comboHotkey1.SelectedItem.ToString(),
            comboModifier2.SelectedItem.ToString() + comboHotkey2.SelectedItem.ToString(),
            comboModifier3.SelectedItem.ToString() + comboHotkey3.SelectedItem.ToString(),
            comboModifier4.SelectedItem.ToString() + comboHotkey4.SelectedItem.ToString(),
            comboModifier5.SelectedItem.ToString() + comboHotkey5.SelectedItem.ToString(),
            comboModifier6.SelectedItem.ToString() + comboHotkey6.SelectedItem.ToString()};
            foreach (var x in a)
                if (hs.Add(x))
                    i++;
            if (i < a.Length) //if not every hotkey combo is unique
            {
                buttonSave.Enabled = false;
                buttonSave.Text = "Hotkey conflict!";
            }
            else
            {
                buttonSave.Enabled = true;
                buttonSave.Text = "Save";
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
