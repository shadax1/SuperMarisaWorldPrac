using System;
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
        static string version = "v1.5";
        static string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string configpath = appdata + @"\SMWPrac\";
        string configfilename = "config.cfg";
        string hotkeyfilename = "hotkey.cfg";
        string savestatesfilename = "savestates.cfg";

        public KeyboardHook hook = new KeyboardHook();
        static int numberHotkeys = 6;
        Keys[] keys = new Keys[numberHotkeys];
        string[] mods = new string[numberHotkeys];

        bool MarisaDoko = false; //false means the pointer offset for stage IDs wasn't found - true means found
        bool MarisaDead = false; //false means the pointer offset for stage IDs wasn't found - true means found
        Stopwatch swSaveState = new Stopwatch();
        #endregion

        #region memory stuff
        static ProcessMemory pm = new ProcessMemory();
        int[] FIRST_OFFSET = { 0x155438 }; //offset to be added to "SuperMarisaWorld.exe" when reading/writing
        int[] FIRST_OFFSET_STAGE_ID = { 0x15542C }; //offset to be added to "SuperMarisaWorld.exe" when reading/writing

        //pointer offsets
        int[] STATE_OFFSET = { -0x30 };
        const int SPAWNING = 1;
        const int PLAYING = 2;
        const int LOADING = 3;
        const int DEAD = 4;
        const int PAUSED = 6;

        int[] SCREEN_ID_OFFSET = { 0x14 }, PIPE_OFFSET = { 0xADC };
        int[] X_OFFSET = { 0x1C }, Y_OFFSET = { 0x28 };
        int[] LIVES_OFFSET = { 0xA88 }, STARS_OFFSET = { 0xA8C }, TIME_OFFSET = { 0xA98 };
        int[] SCORE_OFFSET = { 0xA9C }, HIGHSCORE_OFFSET = { 0xAA0 };
        int[] ANIMATION_OFFSET = { 0x50 }, POWERUP_OFFSET = { 0x54 }, GROUNDED_FLAG_OFFSET = { 0x74 };
        int[] PSPEED_OFFSET = { 0x88 }, FLIGHT_OFFSET = { 0x90 }, IFRAMES_OFFSET = { 0x94 };
        int[] STAGE_ID_OFFSET = { 0x2C8, 0x108, 0x47 }; //the first value can change a lot from a PC to another...

        int storedX = 1, storedY = 1, storedStars = 0, storedTime = 500, storedScore = 0, storedScreenID = 0;
        float XF = 0, YF = 0, storedXF = 1, storedYF = 1; //float coordinates
        int X, Y, stars, time, score, powerup, screenID, pipe, state; //a thread will read various values from the game and store them in these ints here
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

            pSpeedBar.Minimum = 0;
            pSpeedBar.Maximum = 120;
            pSpeedBar.Step = 1;

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

            if (!File.Exists(configpath + savestatesfilename)) //checks if savestatesfilename.cfg exists
                CreateSaveStateTemplateFile();

            toolTip.AutoPopDelay = 10000; toolTip.InitialDelay = 500; toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true; //force the ToolTip text to be displayed whether or not the form is active.
            toolTip.SetToolTip(checkSaveStateTimers, "Displays the save state timer in the 'Score' field inside the HUD\n" +
                                                     "and the time upon entering a pipe in the 'HighScore' field.");
            toolTip.SetToolTip(checkLives, "Freezes lives at the specified value from the text field below.");
            toolTip.SetToolTip(checkStars, "Freezes stars at the specified value from the text field below.");
            toolTip.SetToolTip(checkTime, "Freezes time at the specified value from the text field below.");
            toolTip.SetToolTip(checkScore, "Freezes score at the specified value from the text field below.");
            toolTip.SetToolTip(checkPowerup, "If checked and if Marisa has Miko or broom,\nshe will keep that powerup even after taking a hit.");
            toolTip.SetToolTip(checkPSpeedFly, "If checked and if Marisa has broom, she will be able\nto take off instantly and also have infinite flight.");
            toolTip.SetToolTip(checkIframes, "If checked, gives Marisa infinite invincibility frames.");
            toolTip.SetToolTip(checkSPREADMYWINGS, "If checked, allows Marisa to levitate by holding jump no matter the powerup she has.\nMarisa might glitch visually at times.");
            toolTip.SetToolTip(checkScoreRandomizer, "If checked and if 'Multiples of 10' is selected from the dropdown list\nthe score will have a value multiple of 10 between 0 and 300000 every 0.5s.\n" +
                                                     "If 'Very random' is selected, the score will have a value between 0 and 9999999 every 0.5s.");
            toolTip.SetToolTip(buttonPowerup, "Cycles through powerups in the following order Normal -> Miko -> Broom.\nIf the 'Powerup' checkbox is checked, this button will do nothing.");

            stageList.Add("0-1-1"); stageList.Add("0-1-2"); stageList.Add("0-1-3");
            stageList.Add("0-2-1"); stageList.Add("0-2-2"); stageList.Add("0-2-3");
            stageList.Add("1-1-1"); stageList.Add("1-1-2"); stageList.Add("1-1-3");
            stageList.Add("1-2-1"); stageList.Add("1-2-2"); stageList.Add("1-2-3");
            stageList.Add("2-1-1"); stageList.Add("2-1-2"); stageList.Add("2-1-3");
            stageList.Add("2-2-1"); stageList.Add("2-2-2"); stageList.Add("2-2-3");
            stageList.Add("3-1-1"); stageList.Add("3-1-2"); stageList.Add("3-1-3");
            stageList.Add("3-2-1"); stageList.Add("3-2-2"); stageList.Add("3-2-3");
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
            //if list is null then the stage is only a single screen
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
            new Thread(EnableDisablePopulateControls) { IsBackground = true }.Start();
            new Thread(FindCorrectOffset) { IsBackground = true }.Start();
            new Thread(ReadValues) { IsBackground = true }.Start();
            new Thread(SetFreeze) { IsBackground = true }.Start();
            new Thread(Coordinates) { IsBackground = true }.Start();
            new Thread(PSpeed) { IsBackground = true }.Start();
            new Thread(TrackPowerup) { IsBackground = true }.Start();
            new Thread(TrackDeath) { IsBackground = true }.Start();
            new Thread(TrackPSpeedAfterJump) { IsBackground = true }.Start(); //keeping this as its own thing for accurate readings
            new Thread(Timers) { IsBackground = true }.Start();
            new Thread(ScoreRandomizer) { IsBackground = true }.Start();
        }

        private void CreateSaveStateTemplateFile()
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
        private void EnableDisablePopulateControls()
        {
            int oldScreenID = -1;
            bool buttonsActivated = true, stageStarted = false;

            while (true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (state != 0 && !MarisaDead) //if marisa is no longer on the title screen
                        {
                            if (MarisaDoko)
                            {
                                if (state == PLAYING)
                                {
                                    if (oldScreenID != screenID)
                                    {
                                        oldScreenID = screenID;
                                        foreach (Control c in groupSaves.Controls)
                                        {
                                            if (c is Button) c.BackColor = Color.FromArgb(176, 196, 222);
                                            c.Enabled = true;
                                        }
                                        if (dictScreen[stageID] == null) //if the stage only has a single screen
                                        {
                                            buttonPreviousScreen.Enabled = false; buttonNextScreen.Enabled = false;
                                            buttonPreviousScreen.BackColor = Color.FromArgb(153, 153, 153);
                                            buttonNextScreen.BackColor = Color.FromArgb(153, 153, 153);
                                        }
                                        if (!stageStarted)
                                        {
                                            UpdateComboSaveStates();
                                            comboSaves.SelectedIndex = -1;
                                            comboScoreRandomizer.SelectedIndex = 0;
                                            StoreCoordinates();
                                            stageStarted = true;
                                        }
                                        labelStatus.ForeColor = Color.Gold;
                                        labelStatus.Text = "Marisa is in stage " + stageID + " screen " + screenID;
                                        if (pipe != 0)
                                            labelStatus.Text = "Marisa is in stage " + stageID + " screen " + screenID + " entering pipe " + pipe;
                                        else
                                            labelStatus.Text = "Marisa is in stage " + stageID + " screen " + screenID;
                                    }
                                }
                            }
                            else
                            {
                                foreach (Control c in groupSaves.Controls)
                                {
                                    if (c is Button) c.BackColor = Color.FromArgb(153, 153, 153);
                                    c.Enabled = false;
                                }

                                labelStatus.Text = "Unable to detect where Marisa is. Try launching the game with Win 7 compatibility...";
                                labelStatus.ForeColor = Color.FromArgb(255, 82, 82);
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
                        }
                        else if (X == 0 && Y == 0) //if on title screen or world map
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
                                labelStoredX.Text = "X:"; labelStoredY.Text = "Y:"; textX.Text = ""; textY.Text = "";
                                labelStoredStars.Text = "Stars:"; labelStoredTime.Text = "Time:"; labelStoredScore.Text = "Score:";
                                oldScreenID = -1; buttonsActivated = false; stageStarted = false;
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Caught exception: " + ex.Message);
                }
                Thread.Sleep(100);
            }
        }

        private void FindCorrectOffset() //attempts to find the correct offset for the stage ID pointer
        {
            while (true)
            {
                if (state == PLAYING) //marisa is in a stage
                {
                    int[] LOCAL_STAGE_ID_OFFSET = { 0x0, 0x108, 0x47 }; //temporary array of offsets
                    for (int increment = 0x0; increment < 0x10000; increment += 0x1) //go from 0x0 until 0xFFFF
                    {
                        LOCAL_STAGE_ID_OFFSET[0] = increment; //set the first value of temp array with the increment value
                        byte[] buffer = pm.Read(FIRST_OFFSET_STAGE_ID, LOCAL_STAGE_ID_OFFSET); //read with array offset
                        string localStageID = Encoding.Default.GetString(buffer); //convert result to string
                        if (stageList.Contains(localStageID)) //if result matches a stage ID (ex: 4-1-2)
                        {
                            STAGE_ID_OFFSET = LOCAL_STAGE_ID_OFFSET; //store temp array into the real array
                            MarisaDoko = true;
                            break;
                        }
                    }
                    break; //thread will end after this
                }
                Thread.Sleep(100);
            }
        }

        private void ReadValues() //this thread will read values from the game every millisecond (doesn't require many cpu resources)
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
                buffer = pm.Read(FIRST_OFFSET, PIPE_OFFSET); pipe = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET, STATE_OFFSET); state = BitConverter.ToInt32(buffer, 0);

                buffer = pm.Read(FIRST_OFFSET, SCREEN_ID_OFFSET);
                screenID = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET_STAGE_ID, STAGE_ID_OFFSET);
                stageID = Encoding.Default.GetString(buffer);
                Thread.Sleep(1);
            }
        }

        private void SetFreeze()
        {
            int sleep = 100;
            bool flagPowerup = false;
            byte[] localPowerup = new byte[1]; //rereading them here for consistency

            while (true)
            {
                if (state == PLAYING)
                {
                    sleep = 1;
                    if (checkPSpeedFly.Checked)
                    {
                        pm.Write(FIRST_OFFSET, PSPEED_OFFSET, BitConverter.GetBytes(120));
                        pm.Write(FIRST_OFFSET, FLIGHT_OFFSET, BitConverter.GetBytes(200));
                    }

                    if (checkSPREADMYWINGS.Checked)
                        pm.Write(FIRST_OFFSET, ANIMATION_OFFSET, BitConverter.GetBytes(8));

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
                                    pm.Write(FIRST_OFFSET, POWERUP_OFFSET, localPowerup);
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
                                    catch (Exception)
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
                                    catch (Exception)
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
                                    catch (Exception)
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
                                    catch (Exception)
                                    {
                                        MessageBox.Show("That is not a number", "Nice number",
                                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Caught exception: " + ex.Message);
                    }
                }
                else sleep = 100;
                Thread.Sleep(sleep);
            }
        }

        private void PSpeed()
        {
            byte[] pSpeed = new byte[1];
            while (true)
            {
                pSpeed = pm.Read(FIRST_OFFSET, PSPEED_OFFSET);
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (powerup == 2)
                        {
                            if (!checkPSpeedFly.Checked)
                            {
                                if (pSpeed[0] >= 1 || pSpeed[0] <= 120)
                                {
                                    labelP.Text = pSpeed[0].ToString();
                                    pSpeedBar.Value = pSpeed[0];
                                    if (pSpeed[0] == 120)
                                        pSpeedBar.ForeColor = Color.Blue;
                                    else
                                        pSpeedBar.ForeColor = Color.Green;
                                }
                            }
                            else
                            {
                                labelP.Text = "120";
                                pSpeedBar.Value = 120;
                                pSpeedBar.ForeColor = Color.Blue;
                            }
                        }
                        else if (XF == 0 && YF == 0)
                        {
                            labelP.Text = "";
                            pSpeedBar.Value = 0;
                        }
                        else
                        {
                            labelP.Text = "No broom";
                            pSpeedBar.Value = 120;
                            pSpeedBar.ForeColor = Color.Red;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Caught exception: " + ex.Message);
                }
                Thread.Sleep(1);
            }
        }

        private void TrackPSpeedAfterJump()
        {
            bool flagStore = false;
            byte[] pSpeed = new byte[1];
            byte[] groundedFlag = new byte[1];

            while (true)
            {
                pSpeed = pm.Read(FIRST_OFFSET, PSPEED_OFFSET);
                groundedFlag = pm.Read(FIRST_OFFSET, GROUNDED_FLAG_OFFSET);

                if (powerup != 2)
                    labelJumpP.Invoke((MethodInvoker)delegate () { labelJumpP.Text = ""; });

                if (powerup == 2 && groundedFlag[0] == 0 && !flagStore) //if marisa has her broom and has jumped and the flag is false
                {
                    if (pSpeed[0] != 120)
                        labelJumpP.Invoke((MethodInvoker)delegate () { labelJumpP.Text = "Jumped at: " + pSpeed[0]; });
                    else
                        labelJumpP.Invoke((MethodInvoker)delegate () { labelJumpP.Text = "Jumped at: " + pSpeed[0] + "!!!!"; });
                    flagStore = true;
                }
                if (powerup == 2 && groundedFlag[0] == 1 && flagStore) //if marisa has her broom and has landed
                    flagStore = false; //set the flag back to false

                Thread.Sleep(1);
            }
        }

        private void Coordinates()
        {
            while (true)
            {
                try
                {
                    //update labels
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (state == PLAYING)
                        {
                            labelX.Text = "X: " + XF.ToString("0.000");
                            labelY.Text = "Y: " + YF.ToString("0.000");
                        }
                        else
                        {
                            labelX.Text = "X: 0.000";
                            labelY.Text = "Y: 0.000";
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Caught exception: " + ex.Message);
                }
                Thread.Sleep(50);
            }
        }
        
        private void TrackDeath()
        {
            while (true)
            {
                if (state == DEAD)
                {
                    if (checkRespawn.Checked)
                        LoadStoredValues();
                    else
                    {
                        pm.Write(FIRST_OFFSET, STATE_OFFSET, BitConverter.GetBytes(0));
                        Thread.Sleep(200);
                        pm.Write(FIRST_OFFSET, STATE_OFFSET, BitConverter.GetBytes(2));
                    }
                    MarisaDead = true;
                }
                else MarisaDead = false;
                Thread.Sleep(100);
            }
        }

        private void TrackPowerup()
        {
            while (true)
            {
                if (powerup == 0 && state == PLAYING) //when no powerup -> offer switching to miko
                {
                    buttonPowerup.Invoke((MethodInvoker)delegate () { buttonPowerup.Text = "Power-up: Switch to Miko"; });
                    buttonPowerup.BackColor = Color.FromArgb(255, 153, 153);
                }
                else if (powerup == 1) //when miko -> offer switching to broom
                {
                    buttonPowerup.Invoke((MethodInvoker)delegate () { buttonPowerup.Text = "Power-up: Switch to Broom"; });
                    buttonPowerup.BackColor = Color.FromArgb(255, 209, 146);
                }
                else if (powerup == 2) //when broom -> offer switching to marisa
                {
                    buttonPowerup.Invoke((MethodInvoker)delegate () { buttonPowerup.Text = "Power-up: Back to normal"; });
                    buttonPowerup.BackColor = Color.FromArgb(175, 175, 175);
                }
                else
                {
                    buttonPowerup.Invoke((MethodInvoker)delegate () { buttonPowerup.Text = "Power-up: Back to normal"; });
                    buttonPowerup.BackColor = Color.FromArgb(153, 153, 153);
                }
                Thread.Sleep(100);
            }
        }

        private void Timers()
        {
            Stopwatch swStage = new Stopwatch();
            bool gamePaused = false, gameLoading = false, titleScreen = true, stageStarted = false;
            
            while (true)
            {
                if (state == PLAYING) //if in game
                {
                    if (!stageStarted)
                    {
                        swStage.Restart(); titleScreen = false; stageStarted = true;
                    }
                    if (gamePaused) //resuming the game from the pause screen
                    {
                        swStage.Start(); //resume stage timer
                        if (swSaveState.ElapsedTicks != 0) //if the save state timer was running, resume it too
                            swSaveState.Start();
                        gamePaused = false;
                    }
                    if (gameLoading) //after a loading screen
                    {
                        swStage.Start(); //resume stage timer
                        if (swSaveState.ElapsedTicks != 0) //if the save state timer was running, resume it too
                            swSaveState.Start();
                        gameLoading = false;
                    }
                    labelTimerStage.Invoke((MethodInvoker)delegate () { labelTimerStage.Text = swStage.Elapsed.ToString("mm\\:ss\\.ff"); });
                    labelTimerSaveState.Invoke((MethodInvoker)delegate () { labelTimerSaveState.Text = swSaveState.Elapsed.ToString("mm\\:ss\\.ff"); });
                    if (checkSaveStateTimers.Checked)
                        pm.Write(FIRST_OFFSET, SCORE_OFFSET, BitConverter.GetBytes((int)swSaveState.ElapsedMilliseconds));
                }
                else if (state == LOADING)
                {
                    swStage.Stop(); swSaveState.Stop();
                    labelTimerStageStored.Invoke((MethodInvoker)delegate () { labelTimerStageStored.Text = "(" + swStage.Elapsed.ToString("mm\\:ss\\.ff") + ")"; });
                    labelTimerSaveStateStored.Invoke((MethodInvoker)delegate () { labelTimerSaveStateStored.Text = "(" + swSaveState.Elapsed.ToString("mm\\:ss\\.ff") + ")"; });
                    if (checkSaveStateTimers.Checked)
                        pm.Write(FIRST_OFFSET, HIGHSCORE_OFFSET, BitConverter.GetBytes((int)swSaveState.ElapsedMilliseconds));
                    gameLoading = true;
                }
                else if (state == PAUSED) //if game is paused, pause the timers
                {
                    swStage.Stop(); swSaveState.Stop();
                    gamePaused = true;
                }
                else if (X == 0 && Y == 0) //going back to title screen
                {
                    if (!titleScreen)
                    {
                        swStage.Stop(); swSaveState.Stop();
                        Thread.Sleep(2000); //give the game some time to fully be on the world map
                        labelTimerStageStored.Invoke((MethodInvoker)delegate () { labelTimerStageStored.Text = "(" + swStage.Elapsed.ToString("mm\\:ss\\.ff") + ")"; });
                        labelTimerSaveStateStored.Invoke((MethodInvoker)delegate () { labelTimerSaveStateStored.Text = "(" + swSaveState.Elapsed.ToString("mm\\:ss\\.ff") + ")"; });
                        swStage.Reset(); swSaveState.Reset();
                        labelTimerStage.Invoke((MethodInvoker)delegate () { labelTimerStage.Text = swStage.Elapsed.ToString("mm\\:ss\\.ff"); });
                        labelTimerSaveState.Invoke((MethodInvoker)delegate () { labelTimerSaveState.Text = swSaveState.Elapsed.ToString("mm\\:ss\\.ff"); });
                        titleScreen = true; stageStarted = false;
                    }
                }
                Thread.Sleep(50);
            }
        }

        private void ScoreRandomizer()
        {
            Random random = new Random();
            int sleep = 100, randomNumber = 0;
            while(true)
            {
                try
                {
                    //update labels
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        if (checkScoreRandomizer.Checked)
                        {
                            checkScore.Enabled = false; numericScore.Enabled = false; checkSaveStateTimers.Enabled = false;
                            if (comboScoreRandomizer.Text == "Very random") //generate random numbers
                            {
                                randomNumber = random.Next(0, 9999999);
                                sleep = 500;
                            }
                            else //generate reasonable values that are multiples of 10
                            {
                                randomNumber = random.Next(0, 30000);
                                randomNumber = randomNumber * 10;
                                sleep = 500;
                            }
                            pm.Write(FIRST_OFFSET, SCORE_OFFSET, BitConverter.GetBytes(randomNumber));
                        }
                        else
                        { checkScore.Enabled = true; numericScore.Enabled = true; checkSaveStateTimers.Enabled = true; }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Caught exception: " + ex.Message);
                }
                Thread.Sleep(sleep);
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
            swSaveState.Restart(); //start the save state timer
        }
        #endregion

        #region hotkeys
        private void WriteDefaultHotkeyConfig()
        {
            TextWriter writer = new StreamWriter(configpath + hotkeyfilename);
            writer.WriteLine(version + "\nAlt\nD1\nAlt\nD2\nAlt\nD3\nAlt\nD4\nAlt\nD5\nAlt\nD6");
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
                if (File.ReadLines(configpath + hotkeyfilename).First().Contains(version))
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
            pm.Write(FIRST_OFFSET, STATE_OFFSET, BitConverter.GetBytes(0));
            Thread.Sleep(200);
            pm.Write(FIRST_OFFSET, SCREEN_ID_OFFSET, BitConverter.GetBytes(storedScreenID));
            pm.Write(FIRST_OFFSET, STATE_OFFSET, BitConverter.GetBytes(2));
            pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(storedX));
            pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(storedY));
            pm.Write(FIRST_OFFSET, STARS_OFFSET, BitConverter.GetBytes(storedStars));
            pm.Write(FIRST_OFFSET, TIME_OFFSET, BitConverter.GetBytes(storedTime));
            pm.Write(FIRST_OFFSET, SCORE_OFFSET, BitConverter.GetBytes(storedScore));
        }

        private void ChangePowerup()
        {
            if (powerup == 0)
                pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(1));
            else if (powerup == 1)
                pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(2));
            else
                pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(0));
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
                catch (Exception)
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
                                }
                            }
                            i++;
                        }
                        else //if player is in kaguya's stage -> send them to screen 2 with shmup powerup and specific coordinates
                        {
                            pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(9));
                            pm.Write(FIRST_OFFSET, SCREEN_ID_OFFSET, BitConverter.GetBytes(2));
                            pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(1077411840));
                            pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(1088421888));
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
                                }
                            }
                            i++;
                        }
                        else //if player is in kaguya's stage -> send them to screen 2 with shmup powerup and specific coordinates
                        {
                            pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(9));
                            pm.Write(FIRST_OFFSET, SCREEN_ID_OFFSET, BitConverter.GetBytes(2));
                            pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(1077411840));
                            pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(1088421888));
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

        private void helpAboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }
        #endregion
    }
}
