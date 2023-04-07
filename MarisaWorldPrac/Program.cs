using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperMarisaWorldPrac
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //checking if another instance of the app is running or not
            var mutex = new System.Threading.Mutex(true, "UniqueAppId", out bool result);
            if (!result)
            {
                MessageBox.Show("Another instance of SuperMarisaWorldPrac.exe is already running.",
                                "Instance Duplicate",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }
            GC.KeepAlive(mutex); //mutex shouldn't be released - important line

            //looking for the SuperMarisaWorld.exe process
            Process[] pname = Process.GetProcessesByName("SuperMarisaWorld");
            if (pname.Length == 0)
            {
                MessageBox.Show("Process SuperMarisaWorld.exe wasn't found, make sure the game is running.",
                                "Marisa Not Found",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                Environment.Exit(1);
            }
            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
