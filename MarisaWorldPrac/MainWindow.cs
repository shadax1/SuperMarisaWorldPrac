﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SuperMarisaWorldPrac
{
    public partial class MainWindow : Form
    {
        #region global variables
        static string hotkeyVersion = "v1.5";
        static string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string configpath = appdata + @"\SMWPrac\";
        string configfilename = "config.cfg";
        string hotkeyfilename = "hotkey.cfg";
        string savestatesfilename = "savestates.cfg";

        public KeyboardHook hook = new KeyboardHook();
        static int numberHotkeys = 6;
        Keys[] keys = new Keys[numberHotkeys];
        string[] mods = new string[numberHotkeys];

        Stopwatch swSaveState = new Stopwatch(); //stopwatch that starts upon loading a save state
        #endregion

        #region memory stuff
        static ProcessMemory pm = new ProcessMemory();
        int[] FIRST_OFFSET = { 0x155438 }; //offset to be added to "SuperMarisaWorld.exe" when reading/writing
        int[] FIRST_OFFSET_STAGE_ID = { 0x155424 }; //offset to be added to "SuperMarisaWorld.exe" when reading/writing

        //pointer offsets
        int[] STATE_OFFSET = { -0x30 };
        int[] SCREEN_ID_OFFSET = { 0x14 }, PIPE_OFFSET = { 0xADC }, X_OFFSET = { 0x1C }, Y_OFFSET = { 0x28 };
        int[] LIVES_OFFSET = { 0xA88 }, STARS_OFFSET = { 0xA8C }, TIME_OFFSET = { 0xA98 }, SCORE_OFFSET = { 0xA9C }, SCOREBONUS_OFFSET = { 0xE4 };
        int[] ANIMATION_OFFSET = { 0x50 }, POWERUP_OFFSET = { 0x54 }, RUMIA_OFFSET = { 0x5C }, GROUNDTYPE_OFFSET = { 0x74 };
        int[] PSPEED_OFFSET = { 0x88 }, FLIGHT_OFFSET = { 0x90 }, IFRAMES_OFFSET = { 0x94 }, STOPWATCH_OFFSET = { 0x94C };
        int[] BOMBTIMER_OFFSET = { 0xB00 }, BOMB_OFFSET = { 0xB04 }, FPS_OFFSET = { 0xE80 }, RUNSPEED_OFFSET = { 0x64 };
        int[] STAGE_ID_OFFSET = { 0x80, 0x14, 0x68, 0x108, 0x47 }; //I believe in these offsets, hopefully no more pc shenanigans

        //state constants
        const int NEWSTAGE = 0, SPAWNING = 1, PLAYING = 2, LOADING = 3, DEAD = 4, WIN = 5, PAUSED = 6;
        //powerup constants
        const int MARISA = 0, MIKO = 1, BROOM = 2, RUMIA = 3, SHMUP = 9;
        //animation constants
        const int IDLE = 0, RUN = 1, SLIDE = 3, JUMP = 4, DRILL = 5, BROOM_FLY = 8, BROOM_HOVER = 9, DEATH = 10;
        //bomb block constants
        const int BOMB_ACTIVE = 0, BOMB_INACTIVE = 1;
        //ground type constants
        const int AIR = 0, NORMAL_PLATFORM = 1, MOVING_PLATFORM = 2;

        int storedX = 1, storedY = 1, storedStars = 0, storedTime = 500, storedScore = 0, storedScreenID = 0;

        float XF = 0, YF = 0, storedXF = 1, storedYF = 1; //float coordinates

        //a thread will read various values from the game and store them in these ints here
        int X, Y, stars, time, score, powerup, screenID, pipe, state, groundtype, stopwatch, bomb, rumia, fps;

        string stageID; //a thread will read the stage ID value and store it in this string here
        List<string> stageList = new List<string>();
        Dictionary<string, int[]> dictScreen = new Dictionary<string, int[]>();
        #endregion

        #region MainWindow
        public MainWindow()
        {
            InitializeComponent();
            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;

            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            LoadHotkeys();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(appdata + @"\SMWPrac"))
                Directory.CreateDirectory(appdata + @"\SMWPrac");

            string x, y;
            if (File.Exists(configpath + configfilename)) //checks if config.cfg exists
            {
                using (StreamReader sr = File.OpenText(configpath + configfilename))
                {
                    x = sr.ReadLine();
                    y = sr.ReadLine();
                }
                Location = new Point(int.Parse(x), int.Parse(y)); //places the app at the same position it was in the last time
            }

            toolTip.AutoPopDelay = 20000; toolTip.InitialDelay = 200; toolTip.ReshowDelay = 100;
            toolTip.ShowAlways = true; //force the ToolTip text to be displayed whether or not the form is active
            toolTip.SetToolTip(checkLives, "Freezes lives at the specified value from the text field below.");
            toolTip.SetToolTip(checkStars, "Freezes stars at the specified value from the text field below.");
            toolTip.SetToolTip(checkTime, "Freezes time at the specified value from the text field below.");
            toolTip.SetToolTip(checkScore, "Freezes score at the specified value from the text field below.");
            toolTip.SetToolTip(checkPowerup, "If checked and if Marisa has Miko or broom,\nshe will keep that powerup even after taking a hit.\nIf she has Rumia, she will keep her while this is checked.");
            toolTip.SetToolTip(checkPSpeedFly, "If checked and if Marisa has broom, she will be able\nto take off instantly and also have infinite flight.");
            toolTip.SetToolTip(checkIframes, "If checked, gives Marisa infinite invincibility frames.");
            toolTip.SetToolTip(checkSPREADMYWINGS, "If checked, allows Marisa to levitate by holding jump no matter the powerup she has.\nMarisa might glitch visually at times.");
            toolTip.SetToolTip(checkScoreRandomizer, "If checked and if 'Multiples of 10' is selected from the dropdown list\nthe score will have a value multiple of 10 between 0 and 300000 every 0.5s.\n" +
                                                     "If 'Very random' is selected, the score will have a value between 0 and 9999999 every 0.5s.");
            toolTip.SetToolTip(checkIL, "If checked, this will put Marisa back to the start of the stage upon dying or falling into a pit.");
            toolTip.SetToolTip(buttonPowerup, "Cycles through power-ups in the following order Normal -> Miko -> Broom -> Rumia.\nIf the 'Powerup' checkbox is checked, this button will do nothing.");
            toolTip.SetToolTip(buttonBomb, "Activates or deactivates bomb blocks.");

            if (!File.Exists(configpath + savestatesfilename)) //checks if savestatesfilename.cfg exists
            {
                using (StreamWriter sw = File.CreateText(configpath + savestatesfilename)) //creates the save state file template
                {
                    sw.WriteLine("[0-1-1]\n"); sw.WriteLine("[0-1-2]\n"); sw.WriteLine("[0-1-3]\n"); sw.WriteLine("[0-2-1]\n"); sw.WriteLine("[0-2-2]\n"); sw.WriteLine("[0-2-3]\n");
                    sw.WriteLine("[1-1-1]\n"); sw.WriteLine("[1-1-2]\n"); sw.WriteLine("[1-1-3]\n"); sw.WriteLine("[1-2-1]\n"); sw.WriteLine("[1-2-2]\n"); sw.WriteLine("[1-2-3]\n");
                    sw.WriteLine("[2-1-1]\n"); sw.WriteLine("[2-1-2]\n"); sw.WriteLine("[2-1-3]\n"); sw.WriteLine("[2-2-1]\n"); sw.WriteLine("[2-2-2]\n"); sw.WriteLine("[2-2-3]\n");
                    sw.WriteLine("[3-1-1]\n"); sw.WriteLine("[3-1-2]\n"); sw.WriteLine("[3-1-3]\n"); sw.WriteLine("[3-2-1]\n"); sw.WriteLine("[3-2-2]\n"); sw.WriteLine("[3-2-3]\n");
                    sw.WriteLine("[4-1-1]\n"); sw.WriteLine("[4-1-2]\n"); sw.WriteLine("[4-1-3]\n"); sw.WriteLine("[4-1-4]\n");
                    sw.WriteLine("[4-2-1]\n"); sw.WriteLine("[4-2-2]\n"); sw.WriteLine("[4-2-3]\n"); sw.WriteLine("[4-2-4]\n");
                    sw.WriteLine("[5-1-1]\n"); sw.WriteLine("[5-1-2]\n"); sw.WriteLine("[5-1-3]\n"); sw.WriteLine("[5-1-4]\n");
                    sw.WriteLine("[5-2-1]\n"); sw.WriteLine("[5-2-2]\n"); sw.WriteLine("[5-2-3]\n"); sw.WriteLine("[5-2-4]\n");
                    sw.WriteLine("[6-1-1]\n"); sw.WriteLine("[6-1-2]\n"); sw.WriteLine("[6-1-3]\n"); sw.WriteLine("[6-1-4]\n");
                    sw.WriteLine("[6-2-1]\n"); sw.WriteLine("[6-2-2]\n"); sw.WriteLine("[6-2-3]\n"); sw.WriteLine("[6-2-4]\n");
                    sw.WriteLine("[7-1-1]\n"); sw.WriteLine("[7-1-2]\n"); sw.WriteLine("[7-1-3]\n"); sw.WriteLine("[7-1-4]\n");
                    sw.WriteLine("[7-2-1]\n"); sw.WriteLine("[7-2-2]\n"); sw.WriteLine("[7-2-3]\n"); sw.WriteLine("[7-2-4]\n");
                }
            }
            else //if savestates.cfg exists, run a quick cleanup to remove empty lines that might be in the wrong spots
            {
                List<string> lst_lines = new List<string>();
                using (StreamReader sr = File.OpenText(configpath + savestatesfilename))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line.Length > 0)
                        {
                            if (line.Contains("["))
                            {
                                lst_lines.Add(""); //add an empty line before each stage ID
                                lst_lines.Add(line);
                            }
                            else
                                lst_lines.Add(line); //a save state line
                        }
                    }
                }
                lst_lines.RemoveAt(0); //removes the very first line which is empty
                File.WriteAllLines(configpath + savestatesfilename, lst_lines.ToArray()); //write the new lines to the file
            }

            stageList.Add("0-1-1"); stageList.Add("0-1-2"); stageList.Add("0-1-3"); stageList.Add("0-2-1"); stageList.Add("0-2-2"); stageList.Add("0-2-3");
            stageList.Add("1-1-1"); stageList.Add("1-1-2"); stageList.Add("1-1-3"); stageList.Add("1-2-1"); stageList.Add("1-2-2"); stageList.Add("1-2-3");
            stageList.Add("2-1-1"); stageList.Add("2-1-2"); stageList.Add("2-1-3"); stageList.Add("2-2-1"); stageList.Add("2-2-2"); stageList.Add("2-2-3");
            stageList.Add("3-1-1"); stageList.Add("3-1-2"); stageList.Add("3-1-3"); stageList.Add("3-2-1"); stageList.Add("3-2-2"); stageList.Add("3-2-3");
            stageList.Add("4-1-1"); stageList.Add("4-1-2"); stageList.Add("4-1-3"); stageList.Add("4-1-4");
            stageList.Add("4-2-1"); stageList.Add("4-2-2"); stageList.Add("4-2-3"); stageList.Add("4-2-4");
            stageList.Add("5-1-1"); stageList.Add("5-1-2"); stageList.Add("5-1-3"); stageList.Add("5-1-4");
            stageList.Add("5-2-1"); stageList.Add("5-2-2"); stageList.Add("5-2-3"); stageList.Add("5-2-4");
            stageList.Add("6-1-1"); stageList.Add("6-1-2"); stageList.Add("6-1-3"); stageList.Add("6-1-4");
            stageList.Add("6-2-1"); stageList.Add("6-2-2"); stageList.Add("6-2-3"); stageList.Add("6-2-4");
            stageList.Add("7-1-1"); stageList.Add("7-1-2"); stageList.Add("7-1-3"); stageList.Add("7-1-4");
            stageList.Add("7-2-1"); stageList.Add("7-2-2"); stageList.Add("7-2-3"); stageList.Add("7-2-4");

            //SCREENS - this method kinda sucks but I couldn't come up with anything better
            //dictScreen contains a mapping of screens/pipes per stage -> key is stage name and value is a list with ints
            //if value is null then the stage is only a single screen
            dictScreen.Add("0-1-2", null); dictScreen.Add("0-2-2", null); dictScreen.Add("1-2-1", null);
            dictScreen.Add("2-1-2", null); dictScreen.Add("2-2-2", null); dictScreen.Add("3-1-1", null);
            dictScreen.Add("3-2-2", null); dictScreen.Add("4-2-3", null); dictScreen.Add("5-2-1", null);
            dictScreen.Add("5-2-2", null); dictScreen.Add("6-1-3", null); dictScreen.Add("7-1-1", null);
            dictScreen.Add("7-1-3", null); dictScreen.Add("7-2-3", null);

            //if position in that list is even then it's a screen ID, if it's odd then it's the pipe number leading to that screen ID
            //example: {0,2, 1,1} -> [0] screen 0 is accessed through [1] pipe 2 and [2] screen 1 is accessed through [3] pipe 1
            dictScreen.Add("0-1-1", new int[] {1,1});  dictScreen.Add("0-1-3", new int[] {1,1, 2,2, 3,3, 4,5, 5,7, 6,8, 7,9, 8,11, 0,13});
            dictScreen.Add("0-2-1", new int[] {0,1});  dictScreen.Add("0-2-3", new int[] {0,1});

            dictScreen.Add("1-1-1", new int[] {1,1}); dictScreen.Add("1-1-2", new int[] {0,3}); dictScreen.Add("1-1-3", new int[] {1,1, 2,16});
            dictScreen.Add("1-2-2", new int[] {0,2}); dictScreen.Add("1-2-3", new int[] {2,1, 3,16});

            dictScreen.Add("2-1-1", new int[] {1,1});  dictScreen.Add("2-1-3", new int[] {2,2, 3,1, 0,16});
            dictScreen.Add("2-2-1", new int[] {1,1, 2,2, 3,3});  dictScreen.Add("2-2-3", new int[] {2,1, 0,16});

            dictScreen.Add("3-1-2", new int[] {1,11}); dictScreen.Add("3-1-3", new int[] {1,1, 0,16});
            dictScreen.Add("3-2-1", new int[] {0,2});  dictScreen.Add("3-2-3", new int[] {0,2, 1,5, 3,16});

            dictScreen.Add("4-1-1", new int[] {1,1, 5,2, 3,11, 2,12, 0,14}); dictScreen.Add("4-1-2", new int[] {2,1, 0,11, 3,16});
            dictScreen.Add("4-1-3", new int[] {2,4, 1,6, 3,10}); dictScreen.Add("4-1-4", new int[] {1,1, 2,16});
            dictScreen.Add("4-2-1", new int[] {1,6}); dictScreen.Add("4-2-2", new int[] {0,1, 1,16});  dictScreen.Add("4-2-4", new int[] {6,13, 0,15, 5,16});
            
            dictScreen.Add("5-1-1", new int[] {0,7}); dictScreen.Add("5-1-2", new int[] {0,1, 1,16});
            dictScreen.Add("5-1-3", new int[] {1,1, 2,2, 3,3, 4,4}); dictScreen.Add("5-1-4", new int[] {0,1, 1,16});
            dictScreen.Add("5-2-3", new int[] {1,1, 2,3, 3,4, 4,5, 5,6, 6,7, 7,8, 8,9, 9,10}); dictScreen.Add("5-2-4", new int[] {1,1, 2,5, 3,16});
            
            dictScreen.Add("6-1-1", new int[] {0,1}); dictScreen.Add("6-1-2", new int[] {0,1, 4,2, 2,3, 6,4, 3,5, 5,13, 7,16}); dictScreen.Add("6-1-4", new int[] {2,1, 1,16});
            dictScreen.Add("6-2-1", new int[] {1,1, 2,2, 3,3, 4,4, 5,5, 6,6, 7,7, 8,8, 9,9}); dictScreen.Add("6-2-2", new int[] {2,1, 3,16}); dictScreen.Add("6-2-3", new int[] {0,1});
            dictScreen.Add("6-2-4", new int[] {2}); //2 is kaguya's screen but there is no pipe leading to it

            dictScreen.Add("7-1-2", new int[] {1,1, 0,16}); dictScreen.Add("7-1-4", new int[] {0,1, 1,16});
            dictScreen.Add("7-2-1", new int[] {1,3}); dictScreen.Add("7-2-2", new int[] {2,1, 3,2, 5,3, 6,4, 0,15, 1,16}); dictScreen.Add("7-2-4", new int[] {1,1, 2,16});

            //starting threads
            //enables controls when playing and disables them when title screen/world map
            new Thread(ManageControls) { IsBackground = true }.Start();
            //this thread will read values from the game
            new Thread(ReadValues) { IsBackground = true }.Start();
            //starts a timer upon entering a stage
            new Thread(Timers) { IsBackground = true }.Start();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            using (StreamWriter sw = File.CreateText(configpath + configfilename)) //saving application's position upon closing
            {
                sw.WriteLine(Location.X);
                sw.WriteLine(Location.Y);
            }
        }
        #endregion

        #region threads
        private void ManageControls()
        {
            int oldScreenID = -1;
            bool buttonsActivated = true, stageStarted = false, inReplayOrUserstage = false;
            while (true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (fps != 0) //if marisa is no longer on the title screen
                        {
                            if (state == PLAYING)
                            {
                                if (oldScreenID != screenID)
                                {
                                    try //small try/catch to see if marisa is watching a replay or playing a user stage
                                    {
                                        Console.WriteLine(dictScreen[stageID]);
                                    }
                                    catch (KeyNotFoundException) //exception raised means replay/user stage
                                    {
                                        inReplayOrUserstage = true;
                                        labelStatus.ForeColor = Color.HotPink;
                                        labelStatus.Text = "Marisa is watching a replay or playing a user stage...";
                                    }

                                    oldScreenID = screenID;
                                    if (!inReplayOrUserstage)
                                    {
                                        if (dictScreen[stageID] == null) //if the stage only has a single screen
                                        {
                                            buttonPreviousScreen.Enabled = false; buttonNextScreen.Enabled = false;
                                            buttonPreviousScreen.BackColor = Color.FromArgb(153, 153, 153);
                                            buttonNextScreen.BackColor = Color.FromArgb(153, 153, 153);
                                        }
                                        foreach (Control c in groupSaves.Controls)
                                        {
                                            if (c is Button) c.BackColor = Color.FromArgb(176, 196, 222);
                                            c.Enabled = true;
                                        }
                                    }
                                    if (!stageStarted)
                                    {
                                        UpdateComboSaveStates();
                                        comboSaves.SelectedIndex = -1;
                                        comboScoreRandomizer.SelectedIndex = 0;
                                        StoreCoordinates();
                                        stageStarted = true;
                                        //sets or freezes values based on which checkboxes are checked
                                        new Thread(SetFreeze) { IsBackground = true }.Start();
                                        //updates X/Y labels
                                        new Thread(Coordinates) { IsBackground = true }.Start();
                                        //updates how the power-up button looks based on marisa's current power-up
                                        new Thread(TrackPowerupBomb) { IsBackground = true }.Start();
                                        //respawns marisa at stored coordinates or at the start of the stage if IL mode
                                        new Thread(TrackDeath) { IsBackground = true }.Start();
                                        //tracks marisa's running speed
                                        new Thread(RunSpeed) { IsBackground = true }.Start();
                                    }
                                    if (!buttonsActivated)
                                    {
                                        foreach (Control group in Controls)
                                        {
                                            if (group is GroupBox)
                                            {
                                                if (group.Name != "groupSaves")
                                                {
                                                    foreach (Control c in group.Controls)
                                                    {
                                                        if (c is Button) c.BackColor = Color.FromArgb(176, 196, 222);
                                                        else if (c is CheckBox) ((CheckBox)c).Checked = false;
                                                        c.Enabled = true;
                                                    }
                                                }
                                            }
                                        }
                                        buttonsActivated = true;
                                    }
                                    if (!inReplayOrUserstage)
                                    {
                                        labelStatus.ForeColor = Color.Gold;
                                        labelStatus.Text = "Marisa is in stage " + stageID + " screen " + screenID;
                                    }
                                }
                            }
                            else if (state == LOADING && !inReplayOrUserstage)
                                if (pipe != 0)
                                    labelStatus.Text = "Marisa is in stage " + stageID + " screen " + screenID + " entering pipe " + pipe;
                        }
                        else //if on title screen or world map
                        {
                            if (!stageList.Contains(stageID)) //title screen
                            {
                                labelStatus.Text = "Marisa is on the title screen...";
                                labelStatus.ForeColor = Color.LightGreen;
                            }
                            else //world map
                            {
                                labelStatus.Text = "Marisa is on the world map...";
                                labelStatus.ForeColor = Color.Cyan;
                            }
                            if (buttonsActivated)
                            {
                                foreach (Control group in Controls)
                                {
                                    if (group is GroupBox)
                                    {
                                        foreach (Control c in group.Controls)
                                        {
                                            if (c is Button) c.BackColor = Color.FromArgb(153, 153, 153);
                                            else if (c is CheckBox) ((CheckBox)c).Checked = false;
                                            if (!(c is Label)) c.Enabled = false;
                                        }
                                    }
                                }
                                comboSaves.Items.Clear();
                                labelX.Text = "X:"; labelY.Text = "Y:"; labelP.Text = ""; labelJumpP.Text = ""; labelFlight.Text = "";
                                labelStoredX.Text = "X:"; labelStoredY.Text = "Y:"; textX.Text = ""; textY.Text = ""; buttonPowerup.Text = "Power-up";
                                labelStoredStars.Text = "Stars:"; labelStoredTime.Text = "Time:"; labelStoredScore.Text = "Score:";
                                oldScreenID = -1; buttonsActivated = false; stageStarted = false; inReplayOrUserstage = false;
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        Console.WriteLine(ex.Message);
                }
                Thread.Sleep(100);
            }
        }

        private void ReadValues()
        {
            while (true)
            {
                byte[] buffer = pm.Read(FIRST_OFFSET, X_OFFSET); XF = BitConverter.ToSingle(buffer, 0); //convert to float
                buffer = pm.Read(FIRST_OFFSET, Y_OFFSET); YF = BitConverter.ToSingle(buffer, 0); //convert to float
                buffer = pm.Read(FIRST_OFFSET, X_OFFSET); X = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, Y_OFFSET); Y = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, STARS_OFFSET); stars = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, TIME_OFFSET); time = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, SCORE_OFFSET); score = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, POWERUP_OFFSET); powerup = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, RUMIA_OFFSET); rumia = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, PIPE_OFFSET); pipe = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, BOMB_OFFSET); bomb = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, FPS_OFFSET); fps = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, STATE_OFFSET); state = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, GROUNDTYPE_OFFSET); groundtype = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, SCREEN_ID_OFFSET); screenID = BitConverter.ToInt32(buffer, 0);

                buffer = pm.Read(FIRST_OFFSET_STAGE_ID, STAGE_ID_OFFSET); stageID = Encoding.Default.GetString(buffer);

                int sleep;
                if (fps != 0) sleep = 1; //in-game
                else sleep = 100; //title screen or world map
                Thread.Sleep(sleep);
            }
        }

        private void SetFreeze()
        {
            bool flagPowerup = false, flagRandomizer = false;
            byte[] localPowerup = new byte[1]; //rereading them here for consistency

            while (true)
            {
                if (checkPSpeedFly.Checked)
                {
                    pm.Write(FIRST_OFFSET, PSPEED_OFFSET, BitConverter.GetBytes(120));
                    pm.Write(FIRST_OFFSET, FLIGHT_OFFSET, BitConverter.GetBytes(180));
                }

                if (checkSPREADMYWINGS.Checked)
                    pm.Write(FIRST_OFFSET, ANIMATION_OFFSET, BitConverter.GetBytes(BROOM_FLY));

                if (checkIframes.Checked)
                    pm.Write(FIRST_OFFSET, IFRAMES_OFFSET, BitConverter.GetBytes(180));

                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread and because I access elements in the form
                    {
                        if (checkPowerup.Checked)
                        {
                            buttonPowerup.Enabled = false;
                            if (!flagPowerup)
                            {
                                localPowerup = pm.Read(FIRST_OFFSET, POWERUP_OFFSET);
                                flagPowerup = true;
                            }
                            else
                            {
                                pm.Write(FIRST_OFFSET, POWERUP_OFFSET, localPowerup);
                                if (localPowerup[0] == RUMIA)
                                    pm.Write(FIRST_OFFSET, RUMIA_OFFSET, BitConverter.GetBytes(715));
                            }
                        }
                        else
                        {
                            buttonPowerup.Enabled = true;
                            flagPowerup = false;
                        }

                        if (checkStars.Checked)
                        {
                            if (numericStars.Text.Length > 0)
                            {
                                try
                                {
                                    int desiredStars = int.Parse(numericStars.Text);
                                    pm.Write(FIRST_OFFSET, STARS_OFFSET, BitConverter.GetBytes(desiredStars));
                                }
                                catch (FormatException)
                                {
                                    MessageBox.Show("That is not a number", "Nice number",
                                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }

                        if (checkTime.Checked)
                        {
                            if (numericTime.Text.Length > 0)
                            {
                                try
                                {
                                    int desiredTime = int.Parse(numericTime.Text);
                                    pm.Write(FIRST_OFFSET, TIME_OFFSET, BitConverter.GetBytes(desiredTime));
                                }
                                catch (FormatException)
                                {
                                    MessageBox.Show("That is not a number", "Nice number",
                                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }

                        if (checkScore.Checked)
                        {
                            if (numericScore.Text.Length > 0)
                            {
                                try
                                {
                                    int desiredScore = int.Parse(numericScore.Text);
                                    pm.Write(FIRST_OFFSET, SCORE_OFFSET, BitConverter.GetBytes(desiredScore));
                                }
                                catch (FormatException)
                                {
                                    MessageBox.Show("That is not a number", "Nice number",
                                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }

                        if (checkLives.Checked)
                        {
                            if (numericLives.Text.Length > 0)
                            {
                                try
                                {
                                    int desiredLives = int.Parse(numericLives.Text);
                                    pm.Write(FIRST_OFFSET, LIVES_OFFSET, BitConverter.GetBytes(desiredLives));
                                }
                                catch (FormatException)
                                {
                                    MessageBox.Show("That is not a number", "Nice number",
                                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }

                        if (checkScoreRandomizer.Checked && !flagRandomizer)
                        {
                            flagRandomizer = true;
                            checkScore.Enabled = false; numericScore.Enabled = false;
                            //when checkScoreRandomizer is checked, this will generate random numbers
                            new Thread(ScoreRandomizer) { IsBackground = true }.Start();
                        }
                        else if(!checkScoreRandomizer.Checked && flagRandomizer)
                        { checkScore.Enabled = true; numericScore.Enabled = true; flagRandomizer = false; }
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        Console.WriteLine(ex.Message);
                }

                if (fps == 0)
                    break;
                Thread.Sleep(1);
            }
        }

        private void RunSpeed()
        {
            byte[] speed = new byte[1]; float speedF;
            while (true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        speed = pm.Read(FIRST_OFFSET, RUNSPEED_OFFSET);
                        speedF = BitConverter.ToSingle(speed, 0); //convert to float
                        if (speedF < 0) speedF = speedF * -10000;
                        else speedF = speedF * 10000;
                        if ((int)speedF <= 1875)
                            runSpeedBar.Value = (int)speedF;
                            if ((int)speedF == 1875)
                                runSpeedBar.ForeColor = Color.Blue;
                            else
                                runSpeedBar.ForeColor = Color.Green;

                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        Console.WriteLine(ex.Message);
                }

                if (fps == 0)
                    break;
                Thread.Sleep(10);
            }
        }

        private void PSpeed()
        {
            byte[] pSpeed = new byte[1];
            while (true)
            {
                if (powerup == BROOM)
                {
                    try
                    {
                        Invoke((MethodInvoker)delegate //using this because thread
                        {
                            if (!checkPSpeedFly.Checked)
                            {
                                pSpeed = pm.Read(FIRST_OFFSET, PSPEED_OFFSET);
                                if (pSpeed[0] >= 1 || pSpeed[0] <= 120)
                                {
                                    labelP.Text = pSpeed[0].ToString() + "/120";
                                    pSpeedBar.Value = pSpeed[0];
                                    if (pSpeed[0] == 120)
                                        pSpeedBar.ForeColor = Color.Blue;
                                    else
                                        pSpeedBar.ForeColor = Color.Green;
                                }
                            }
                            else
                            {
                                labelP.Text = "120/120";
                                pSpeedBar.Value = 120;
                                pSpeedBar.ForeColor = Color.Blue;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        if (ex is ObjectDisposedException || ex is InvalidOperationException)
                            Console.WriteLine(ex.Message);
                    }
                }
                else
                    break;
                Thread.Sleep(10);
            }
        }

        private void Flight()
        {
            byte[] flight = new byte[1];
            byte[] animation = new byte[1];
            while (true)
            {
                if (powerup == BROOM)
                {
                    try
                    {
                        Invoke((MethodInvoker)delegate //using this because thread
                        {
                            if (!checkPSpeedFly.Checked)
                            {
                                flight = pm.Read(FIRST_OFFSET, FLIGHT_OFFSET);
                                animation = pm.Read(FIRST_OFFSET, ANIMATION_OFFSET);
                                if (animation[0] == BROOM_FLY && flight[0] >= 0 && flight[0] <= 180)
                                {
                                    flightBar.Value = flight[0];
                                    labelFlight.Text = flight[0].ToString() + "/180";
                                }
                                else
                                {
                                    labelFlight.Text = "";
                                    flightBar.Value = 0;
                                }
                            }
                            else
                            {
                                labelFlight.Text = "180/180";
                                flightBar.Value = 180;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        if (ex is ObjectDisposedException || ex is InvalidOperationException)
                            Console.WriteLine(ex.Message);
                    }
                }
                else
                    break;
                Thread.Sleep(10);
            }
        }
        
        private void TrackPSpeedAfterJump()
        {
            bool flagStore = false;
            byte[] pSpeed = new byte[1];
            byte[] groundedFlag = new byte[1];
            labelJumpP.BackColor = Color.Transparent;
            while (true)
            {
                if (powerup == BROOM)
                {
                    try
                    {
                        Invoke((MethodInvoker)delegate //using this because thread
                        {
                            pSpeed = pm.Read(FIRST_OFFSET, PSPEED_OFFSET);
                            groundedFlag = pm.Read(FIRST_OFFSET, GROUNDTYPE_OFFSET);

                            if (groundedFlag[0] == 0 && !flagStore) //if marisa has jumped and the flag is false
                            {
                                if (pSpeed[0] != 120)
                                    labelJumpP.Text = pSpeed[0].ToString();
                                else
                                    labelJumpP.Text = pSpeed[0].ToString() + "!!";
                                flagStore = true;
                            }
                            if (groundedFlag[0] == 1 && flagStore) //if marisa has landed
                                flagStore = false; //set the flag back to false
                        });
                    }
                    catch (Exception ex)
                    {
                        if (ex is ObjectDisposedException || ex is InvalidOperationException)
                            Console.WriteLine(ex.Message);
                    }
                }
                else
                    break;
                Thread.Sleep(1);
            }
        }

        private void Rumia()
        {
            float rumiaTimer;
            int rumiaMaximum = 715;
            while (true)
            {
                if (powerup == RUMIA)
                {
                    try
                    {
                        Invoke((MethodInvoker)delegate //using this because thread
                        {

                            if (rumia >= 0 && rumia <= rumiaMaximum)
                            {
                                flightBar.Value = rumia;
                                flightBar.ForeColor = Color.FromArgb(255, 175, 59);
                                rumiaTimer = (float)rumia / 60;
                                labelFlight.Text = rumiaTimer.ToString("0.00");
                            }
                            else if (rumia > rumiaMaximum)
                            {
                                rumiaMaximum = rumia;
                                flightBar.Maximum = rumiaMaximum;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        if (ex is ObjectDisposedException || ex is InvalidOperationException)
                            Console.WriteLine(ex.Message);
                    }
                }
                else
                    break;
                Thread.Sleep(10);
            }
        }

        private void Coordinates()
        {
            while (true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        labelX.Text = "X: " + XF.ToString("0.000");
                        labelY.Text = "Y: " + YF.ToString("0.000");
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        Console.WriteLine(ex.Message);
                }
                
                if (fps == 0)
                    break;
                Thread.Sleep(50);
            }
        }
        
        private void TrackDeath()
        {
            while (true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (state == DEAD || YF < 0)
                        {
                            if (checkIL.Checked) //IL mode - only restart the stage upon death
                            {
                                pm.Write(FIRST_OFFSET, STATE_OFFSET, BitConverter.GetBytes(NEWSTAGE));
                                if (groundtype == MOVING_PLATFORM) //mokou's room
                                    pm.Write(FIRST_OFFSET, GROUNDTYPE_OFFSET, BitConverter.GetBytes(AIR)); //not grounded otherwise game crashes
                                pm.Write(FIRST_OFFSET, SCOREBONUS_OFFSET, BitConverter.GetBytes(1000));
                                pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(MARISA));
                                pm.Write(FIRST_OFFSET, STOPWATCH_OFFSET, BitConverter.GetBytes(0));
                            }
                            else //not IL mode, reload stored values upon death
                                LoadStoredValues();
                        }
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        Console.WriteLine(ex.Message);
                }
                
                if (fps == 0)
                    break;
                Thread.Sleep(10);
            }
        }
        
        private void TrackPowerupBomb()
        {
            bool flagRumia = false;
            bool flagBroom = false;

            while (true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (powerup == MARISA) //when no powerup -> offer switching to miko
                        {
                            labelP.Text = ""; pSpeedBar.Value = 0; labelJumpP.Text = "";
                            labelFlight.Text = ""; flightBar.Value = 0;

                            buttonPowerup.Text = "Power-up: Switch to Miko";
                            buttonPowerup.BackColor = Color.FromArgb(255, 153, 153);
                            if (flagBroom || flagRumia) { flagBroom = false; flagRumia = false; }
                        }
                        else if (powerup == MIKO) //when miko -> offer switching to broom
                        {
                            labelP.Text = ""; pSpeedBar.Value = 0; labelJumpP.Text = "";
                            labelFlight.Text = ""; flightBar.Value = 0;

                            buttonPowerup.Text = "Power-up: Switch to Broom";
                            buttonPowerup.BackColor = Color.FromArgb(240, 190, 80);
                            if (flagBroom || flagRumia) { flagBroom = false; flagRumia = false; }
                        }
                        else if (powerup == BROOM) //when broom -> offer switching to rumia
                        {
                            if (!flagBroom)
                            {
                                flightBar.Maximum = 180; flightBar.ForeColor = Color.DarkBlue;
                                //keeps track of marisa's p-speed
                                new Thread(PSpeed) { IsBackground = true }.Start();
                                //keeps track of marisa's flight time after p-speed
                                new Thread(Flight) { IsBackground = true }.Start();
                                //tells at what point in the pspeed meter marisa jumped - keeping this as its own thing for accurate results
                                new Thread(TrackPSpeedAfterJump) { IsBackground = true }.Start();
                                flagBroom = true;
                                flagRumia = false;
                            }
                            buttonPowerup.Text = "Power-up: Switch to Rumia";
                            buttonPowerup.BackColor = Color.FromArgb(255, 230, 80);
                        }
                        else if (powerup == RUMIA) //when rumia -> offer switching back to normal
                        {
                            if (!flagRumia)
                            {
                                flightBar.Maximum = 715;
                                labelJumpP.Text = ""; labelP.Text = ""; pSpeedBar.Value = 0;
                                //keeps track of rumia's duration
                                new Thread(Rumia) { IsBackground = true }.Start();
                                flagRumia = true;
                                flagBroom = false;
                            }
                            buttonPowerup.Text = "Power-up: Back to normal";
                            buttonPowerup.BackColor = Color.FromArgb(153, 153, 153);
                        }
                        else
                        {
                            labelP.Text = ""; pSpeedBar.Value = 0; labelJumpP.Text = "";
                            labelFlight.Text = ""; flightBar.Value = 0;

                            buttonPowerup.Text = "Power-up: Back to normal";
                            buttonPowerup.BackColor = Color.FromArgb(153, 153, 153);
                            if (flagBroom || flagRumia) { flagBroom = false; flagRumia = false; }
                        }

                        if (bomb == BOMB_ACTIVE)
                        {
                            buttonBomb.Image = Properties.Resources.BombBlock2;
                            buttonBomb.ImageAlign = ContentAlignment.MiddleCenter;
                        }
                        else
                        {
                            buttonBomb.Image = Properties.Resources.BombBlock;
                            buttonBomb.ImageAlign = ContentAlignment.MiddleCenter;
                        }
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        Console.WriteLine(ex.Message);
                }
                
                if (fps == 0)
                    break;
                Thread.Sleep(100);
            }
        }

        private void Timers()
        {
            TimeSpan t = new TimeSpan();
            
            while (true)
            {
                byte[] buffer = pm.Read(FIRST_OFFSET, STOPWATCH_OFFSET); stopwatch = BitConverter.ToInt32(buffer, 0);
                t = TimeSpan.FromSeconds((double)stopwatch / 120); //stopatch value counts seconds per 120 units for some reason
                string stringStopwatch = t.Minutes.ToString("D2") + ":" + t.Seconds.ToString("D2") + "." + t.Milliseconds.ToString("D3");
                if (state == NEWSTAGE) //new stage
                    if (swSaveState.ElapsedTicks != 0) //if the save state timer was running, reset it
                        swSaveState.Reset();
                if (state == PLAYING) //if in game
                {
                    if (swSaveState.ElapsedTicks != 0) //if the save state timer was running, resume it too
                        swSaveState.Start();
                    try
                    {
                        Invoke((MethodInvoker)delegate //using this because thread
                        {
                            labelTimerStage.Text = stringStopwatch;
                            labelTimerSaveState.Text = swSaveState.Elapsed.ToString("mm\\:ss\\.fff");
                        });
                    }
                    catch (Exception ex)
                    {
                        if (ex is ObjectDisposedException || ex is InvalidOperationException)
                            Console.WriteLine(ex.Message);
                    }
                }
                //if game is loading, paused or on the title screen -> pause timers
                else if (state == LOADING || state == PAUSED || fps == 0)
                    swSaveState.Stop();
                Thread.Sleep(20);
            }
        }

        private void ScoreRandomizer()
        {
            Random random = new Random();
            int randomNumber = 0;
            while(true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (comboScoreRandomizer.Text == "Very random") //generate random numbers
                            randomNumber = random.Next(0, 9999999);
                        else //generate reasonable values that are multiples of 10
                        {
                            randomNumber = random.Next(0, 30000);
                            randomNumber = randomNumber * 10;
                        }
                        pm.Write(FIRST_OFFSET, SCORE_OFFSET, BitConverter.GetBytes(randomNumber));
                    });
                }
                catch (Exception ex)
                {
                    if (ex is ObjectDisposedException || ex is InvalidOperationException)
                        Console.WriteLine(ex.Message);
                }

                if (!checkScoreRandomizer.Checked)
                    break;
                Thread.Sleep(500);
            }
        }
        #endregion

        #region states
        private void UpdateComboSaveStates()
        {
            if (stageList.Contains(stageID))
            {
                bool sectionFound = false;
                if (File.Exists(configpath + savestatesfilename)) //checks if savestates.cfg exists
                {
                    comboSaves.Items.Clear();
                    using (StreamReader sr = File.OpenText(configpath + savestatesfilename))
                    {
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();

                            //skip empty lines
                            if (line.Length > 0)
                            {
                                //if another section is reached after the desired one is parsed
                                if (line.Contains("[") && sectionFound)
                                    break;

                                //if flag is true then analyze the line to check its screenID
                                if (sectionFound)
                                    comboSaves.Items.Add(line);

                                //if reached the desired section -> set flag to true
                                if (line.Contains(stageID))
                                    sectionFound = true;
                            }
                        }
                        comboSaves.SelectedIndex = comboSaves.Items.Count - 1; //this removes the empty entry
                    }
                }
            }
        }

        private void comboSaves_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string[] split = comboSaves.Text.Split('|');
            string[] split2 = split[1].Split(',');

            //convert the floats to bytes
            byte[] byteX = BitConverter.GetBytes(float.Parse(split2[0].Trim()));
            byte[] byteY = BitConverter.GetBytes(float.Parse(split2[1].Trim()));

            //convert the bytes to int and store them
            storedX = BitConverter.ToInt32(byteX, 0); //convert to int globally
            storedY = BitConverter.ToInt32(byteY, 0); //convert to int globally
            storedXF = BitConverter.ToSingle(byteX, 0); //convert to float globally
            storedYF = BitConverter.ToSingle(byteY, 0); //convert to float globally

            labelStoredX.Text = "X: " + float.Parse(split2[0].Trim()).ToString("0.000"); //X
            labelStoredY.Text = "Y: " + float.Parse(split2[1].Trim()).ToString("0.000"); //Y

            if (split2.Length > 3) //stars, time and score are stored
            {
                storedStars = int.Parse(split2[2]); //stars
                storedTime = int.Parse(split2[3]); //time
                storedScore = int.Parse(split2[4]); //score
            }
            else //if none of stars/time/score was stored, just use the current ones
            {
                storedStars = stars; storedTime = time; storedScore = score;
            }
            storedScreenID = comboSaves.Text.Last() - '0'; //apparently this converts a char to an int, thanks google
            labelStoredStars.Text = "Stars: " + storedStars;
            labelStoredTime.Text = "Time: " + storedTime;
            labelStoredScore.Text = "Score: " + storedScore;

            LoadStoredValues();
        }
        #endregion

        #region hotkeys
        private void WriteDefaultHotkeyConfig()
        {
            TextWriter writer = new StreamWriter(configpath + hotkeyfilename);
            writer.WriteLine(hotkeyVersion + "\nAlt\nD1\nAlt\nD2\nAlt\nD3\nAlt\nD4\nAlt\nD5\nAlt\nD6");
            writer.Close();
        }

        private void LoadHotkeys()
        {
            hook.SimpleDispose();
            if (!Directory.Exists(configpath)) Directory.CreateDirectory(configpath);
            if (!File.Exists(configpath + hotkeyfilename))
                WriteDefaultHotkeyConfig();
            if (File.Exists(configpath + hotkeyfilename)) //checks if hotkey.cfg exists
            {
                if (File.ReadLines(configpath + hotkeyfilename).First().Contains(hotkeyVersion))
                {
                    using (StreamReader sr = File.OpenText(configpath + hotkeyfilename))
                    {
                        sr.ReadLine();
                        for (int i = 2, m = 0, k = 0; i <= numberHotkeys * 2 + 1; i++)
                        {
                            if (i % 2 == 0) //if the line number is even then it's a modifier
                            {
                                mods[m] = sr.ReadLine();
                                m++;
                            }
                            else //if the line number is odd then it's a hotkey
                            {
                                KeysConverter kc = new KeysConverter();
                                keys[k] = (Keys)kc.ConvertFromString(sr.ReadLine());
                                k++;
                            }
                        }
                    }
                    //for (int i = 0; i < mods.Length; i++) Console.WriteLine("mods= " + mods[i]);
                    //for (int i = 0; i < keys.Length; i++) Console.WriteLine("keys= " + keys[i]);
                    for (int i = 0; i < mods.Length; i++) //register each hotkey
                    {
                        try
                        {
                            if (mods[i] == "None")
                                hook.RegisterHotKey(0, keys[i]);
                            else if (mods[i] == "Alt")
                                hook.RegisterHotKey(SuperMarisaWorldPrac.ModifierKeys.Alt, keys[i]);
                            else if (mods[i] == "Control")
                                hook.RegisterHotKey(SuperMarisaWorldPrac.ModifierKeys.Control, keys[i]);
                            else if (mods[i] == "Shift")
                                hook.RegisterHotKey(SuperMarisaWorldPrac.ModifierKeys.Shift, keys[i]);
                        }
                        catch (InvalidOperationException)
                        {
                            MessageBox.Show("Another application is already using the following hotkey combination: " +
                                            mods[i] + " " + keys[i] + ".\n" +
                                            "Hotkeys have been set back to defaults.\n",
                                            "Hotkey conflict detected",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);
                            WriteDefaultHotkeyConfig();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Some changes have been made to hotkeys. They have been set back to defaults.\n",
                                    "Hotkeys changed",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    WriteDefaultHotkeyConfig();
                }
            }
        }
        
        private void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            for (int i = 0; i < numberHotkeys; i++)
            {
                if (mods[i] == "Alt" && e.Key == keys[i] || 
                    mods[i] == "Control" && e.Key == keys[i] || 
                    mods[i] == "Shift" && e.Key == keys[i] || 
                    e.Key == keys[i])
                {
                    if (i == 0 && state == PLAYING) ChangePowerup();
                    if (i == 1 && state == PLAYING) { StoreCoordinates(); comboSaves.SelectedIndex = -1; }
                    if (i == 2 && state == PLAYING) LoadStoredValues();
                    if (i == 3 && state == PLAYING) LoadNextSaveState();
                    if (i == 4 && state == PLAYING) buttonPreviousScreen_Click(null, null);
                    if (i == 5 && state == PLAYING) buttonNextScreen_Click(null, null);
                }
            }
        }
        #endregion

        #region actions
        private void StoreCoordinates()
        {
            if (state == PLAYING)
            {
                byte[] xPos = pm.Read(FIRST_OFFSET, X_OFFSET); //read x speed value
                byte[] yPos = pm.Read(FIRST_OFFSET, Y_OFFSET); //read y speed value

                storedX = BitConverter.ToInt32(xPos, 0); //convert to int globally
                storedY = BitConverter.ToInt32(yPos, 0); //convert to int globally
                storedXF = BitConverter.ToSingle(xPos, 0); //convert to float globally
                storedYF = BitConverter.ToSingle(yPos, 0); //convert to float globally
                storedStars = stars; storedTime = time; storedScore = score; storedScreenID = screenID;

                labelStoredX.Text = "X: " + XF.ToString("0.000");
                labelStoredY.Text = "Y: " + YF.ToString("0.000");
                labelStoredStars.Text = "Stars: " + storedStars;
                labelStoredTime.Text = "Time: " + storedTime;
                labelStoredScore.Text = "Score: " + storedScore;
            }
        }

        private void LoadStoredValues()
        {
            pm.Write(FIRST_OFFSET, STATE_OFFSET, BitConverter.GetBytes(NEWSTAGE));
            if (stageID == "7-2-3") //this stage requires a slightly longer sleep, probably because the stage is so huge
                Thread.Sleep(300);
            else if ((stageID == "4-2-4" && screenID == 5) || //remilia's room
                     (stageID == "5-1-4" && screenID == 1) || //youmu's room
                     (stageID == "5-2-4" && screenID == 3) || //yuyuko's room
                     (stageID == "7-2-2" && screenID == 1))   //yukari's room
                Thread.Sleep(1000); //loading values in these rooms require a longer sleep
            else //regular stage - regular sleep
                Thread.Sleep(200);
            if (groundtype == MOVING_PLATFORM) //if marisa on a moving platform
                pm.Write(FIRST_OFFSET, GROUNDTYPE_OFFSET, BitConverter.GetBytes(AIR)); //set ground type to air (if save state was grounded, it will immediately be set to 1 by the game)
            pm.Write(FIRST_OFFSET, SCREEN_ID_OFFSET, BitConverter.GetBytes(storedScreenID));
            pm.Write(FIRST_OFFSET, STATE_OFFSET, BitConverter.GetBytes(PLAYING));
            pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(storedX));
            pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(storedY));
            pm.Write(FIRST_OFFSET, STARS_OFFSET, BitConverter.GetBytes(storedStars));
            pm.Write(FIRST_OFFSET, TIME_OFFSET, BitConverter.GetBytes(storedTime));
            pm.Write(FIRST_OFFSET, SCORE_OFFSET, BitConverter.GetBytes(storedScore));
            swSaveState.Restart(); //start the save state timer
        }

        private void ChangePowerup()
        {
            if (powerup == MARISA)
                pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(MIKO));
            else if (powerup == MIKO)
                pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(BROOM));
            else if (powerup == BROOM)
            {
                pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(RUMIA));
                pm.Write(FIRST_OFFSET, RUMIA_OFFSET, BitConverter.GetBytes(715));
            }
            else
                pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(MARISA));
        }

        private void LoadNextSaveState()
        {
            if (comboSaves.Items.Count > 0)
            {
                if (comboSaves.SelectedIndex < comboSaves.Items.Count - 1)
                    comboSaves.SelectedIndex += 1;
                else if (comboSaves.SelectedIndex == comboSaves.Items.Count - 1 || comboSaves.SelectedIndex == -1)
                    comboSaves.SelectedIndex = 0;
                comboSaves_SelectionChangeCommitted(null, null);
            }
        }
        #endregion

        #region buttons
        private void buttonGo_Click(object sender, EventArgs e)
        {
            if (textX.Text.Length > 0 && textY.Text.Length > 0 && state == PLAYING)
            {
                try
                {
                    //get values entered as float
                    float desiredX = float.Parse(textX.Text, CultureInfo.InvariantCulture.NumberFormat);
                    float desiredY = float.Parse(textY.Text, CultureInfo.InvariantCulture.NumberFormat);

                    if (desiredX > 400 || desiredY > 400)
                    {
                        DialogResult dialogResult = 
                            MessageBox.Show("These coordinates might crash the game, are you sure?",
                                            "That's a big number",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Exclamation);
                        if (dialogResult == DialogResult.Yes)
                        {
                            pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(desiredX));
                            pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(desiredY));
                        }
                    }
                    else
                    {
                        pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(desiredX));
                        pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(desiredY));
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("That is not a number",
                                    "Nice number",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                }
            }
        }
        
        private void buttonStore_Click(object sender, EventArgs e)
        {
            if (state == PLAYING)
            {
                StoreCoordinates();
                comboSaves.SelectedIndex = -1;
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (state == PLAYING)
                LoadStoredValues();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (stageList.Contains(stageID) && state == PLAYING)
            {
                using (SaveStateName ssname = new SaveStateName())
                {
                    if (ssname.ShowDialog() == DialogResult.OK)
                    {
                        bool sectionFound = false;
                        int lineNumber = 0;

                        if (File.Exists(configpath + savestatesfilename)) //checks if savestates.cfg exists
                        {
                            //find the line number to which the new line needs to be inserted
                            using (StreamReader sr = File.OpenText(configpath + savestatesfilename))
                            {
                                while (!sr.EndOfStream)
                                {
                                    string line = sr.ReadLine();

                                    if (line.Length > 0)
                                        if (line.Contains("[") && sectionFound)
                                            break;
                                    if (line.Contains(stageID))
                                        sectionFound = true;
                                    lineNumber++;
                                }
                            }
                            //read all save state lines into memory
                            var lines = File.ReadAllLines(configpath + savestatesfilename).ToList();
                            //insert the desired line at the number found - 1
                            if (checkSave.Checked)
                                lines.Insert(lineNumber - 1, ssname.name + " | " + XF.ToString("0.000") + "," + YF.ToString("0.000") + "," + stars + "," + time + "," + score + "," + screenID);
                            else
                                lines.Insert(lineNumber - 1, ssname.name + " | " + XF.ToString("0.000") + "," + YF.ToString("0.000") + "," + screenID);
                            File.WriteAllLines(configpath + savestatesfilename, lines); //write the new lines to the file
                            UpdateComboSaveStates();
                            StoreCoordinates();
                        }
                    }
                }
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (File.Exists(configpath + savestatesfilename)) //checks if savestates.cfg exists
            {
                int lineNumber = 0;
                //find the line number of the line to delete
                using (StreamReader sr = File.OpenText(configpath + savestatesfilename))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        //leave the loop once the line to remove is found
                        if (line == comboSaves.Text)
                            break;
                        lineNumber++;
                    }
                }
                //read all save state lines into memory
                var lines = File.ReadAllLines(configpath + savestatesfilename).ToList();
                lines.RemoveAt(lineNumber);
                File.WriteAllLines(configpath + savestatesfilename, lines); //write the new lines to the file
                UpdateComboSaveStates();
            }
        }

        private void buttonPowerup_Click(object sender, EventArgs e)
        {
            ChangePowerup();
        }

        private void buttonBomb_Click(object sender, EventArgs e)
        {
            if (state == PLAYING)
            {
                if (bomb == BOMB_INACTIVE)
                {
                    pm.Write(FIRST_OFFSET, BOMB_OFFSET, BitConverter.GetBytes(BOMB_ACTIVE));
                    pm.Write(FIRST_OFFSET, BOMBTIMER_OFFSET, BitConverter.GetBytes(600));
                }
                else
                {
                    pm.Write(FIRST_OFFSET, BOMB_OFFSET, BitConverter.GetBytes(BOMB_INACTIVE));
                }
            }
        }

        private void buttonPreviousScreen_Click(object sender, EventArgs e)
        {
            if (stageList.Contains(stageID))
            {
                if (dictScreen[stageID] != null)
                {
                    int i = 0;
                    bool currentScreenFound = false;
                    foreach (int screen_pipe in dictScreen[stageID]) //search for the current screen in the list
                    {
                        if (stageID != "6-2-4")
                        {
                            if (i % 2 == 0) //i is an even number -> dealing with a screen
                            {
                                if (screen_pipe == screenID) //if marisa's current screen is in the list
                                {
                                    if (i - 1 >= 0) //make sure we don't overshoot the array positions
                                    {
                                        //send her to the previous screen, aka the previous pipe which is 1 position before
                                        pm.Write(FIRST_OFFSET, PIPE_OFFSET, BitConverter.GetBytes(dictScreen[stageID][i - 1]));
                                        currentScreenFound = true;
                                        break;
                                    }
                                    else //marisa is in the screen just after the spawn screen -> send her back to spawn
                                    {
                                        pm.Write(FIRST_OFFSET, STATE_OFFSET, BitConverter.GetBytes(NEWSTAGE));
                                        currentScreenFound = true;
                                        break;
                                    }
                                }
                            }
                            i++;
                        }
                        else //if marisa is in kaguya's stage -> send them to screen 2 with shmup powerup and specific coordinates
                        {
                            pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(SHMUP));
                            pm.Write(FIRST_OFFSET, SCREEN_ID_OFFSET, BitConverter.GetBytes(2));
                            pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(1077411840)); //specific x coord
                            pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(1088421888)); //specific y coord
                            currentScreenFound = true;
                        }
                    }
                    //when current screen isn't in the list, marisa is in the spawn screen
                    //and when marisa is in the last possible screen, this bool will remain false, so this loops back to the last possible screen
                    if (!currentScreenFound)
                        //send her to the last screen possible, aka last pipe which is always in position last position [-1]
                        pm.Write(FIRST_OFFSET, PIPE_OFFSET, BitConverter.GetBytes(dictScreen[stageID].Last()));
                }
            }
        }

        private void buttonNextScreen_Click(object sender, EventArgs e)
        {
            if (stageList.Contains(stageID))
            {
                if (dictScreen[stageID] != null)
                {
                    int i = 0;
                    bool currentScreenFound = false;
                    foreach (int screen_pipe in dictScreen[stageID]) //search for the current screen in the list
                    {
                        if (stageID != "6-2-4")
                        {
                            if (i % 2 == 0) //i is an even number -> dealing with a screen
                            {
                                if (screen_pipe == screenID) //if marisa's current screen is in the list
                                {
                                    if (i + 3 <= dictScreen[stageID].Length) //make sure we don't overshoot the array positions
                                    {
                                        //send her to the next screen, aka the next pipe which is 3 positions away
                                        pm.Write(FIRST_OFFSET, PIPE_OFFSET, BitConverter.GetBytes(dictScreen[stageID][i + 3]));
                                        currentScreenFound = true;
                                        break;
                                    }
                                    else //marisa is in the last screen possible -> send her back to spawn
                                    {
                                        pm.Write(FIRST_OFFSET, STATE_OFFSET, BitConverter.GetBytes(NEWSTAGE));
                                        currentScreenFound = true;
                                        break;
                                    }
                                }
                            }
                            i++;
                        }
                        else //if marisa is in kaguya's stage -> send them to screen 2 with shmup powerup and specific coordinates
                        {
                            pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(SHMUP));
                            pm.Write(FIRST_OFFSET, SCREEN_ID_OFFSET, BitConverter.GetBytes(2));
                            pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(1077411840)); //specific x coord
                            pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(1088421888)); //specific y coord
                            currentScreenFound = true;
                        }
                    }
                    //when current screen isn't in the list, marisa is in the spawn screen
                    //and when marisa is in the last possible screen, this bool will remain false, so this loops back to the first possible screen
                    if (!currentScreenFound)
                        //send her to the first screen possible, aka first pipe which is always in position [1]
                        pm.Write(FIRST_OFFSET, PIPE_OFFSET, BitConverter.GetBytes(dictScreen[stageID][1]));
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HotkeyDialog hd = new HotkeyDialog();
            hd.ShowDialog();
            LoadHotkeys();
        }

        private void applicationFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", configpath);
        }

        private void helpAboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }
        #endregion
    }
}
