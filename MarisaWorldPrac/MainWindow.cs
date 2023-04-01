using System;
using System.Collections.Generic;
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
        static string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string configpath = appdata + @"\SMWPrac\";
        string configfilename = "config.cfg";
        string hotkeyfilename = "hotkey.cfg";
        string savestatesfilename = "savestates.cfg";

        public KeyboardHook hook = new KeyboardHook();
        static int numberHotkeys = 7;
        Keys[] keys = new Keys[numberHotkeys];
        string[] mods = new string[numberHotkeys];
        #endregion

        #region memory stuff
        static ProcessMemory pm = new ProcessMemory();
        int[] FIRST_OFFSET = { 0x155438 }; //offset to be added to "SuperMarisaWorld.exe" when reading/writing
        int[] FIRST_OFFSET_STAGE_ID = { 0x15542C }; //offset to be added to "SuperMarisaWorld.exe" when reading/writing

        //pointer offsets
        int[] STATE_OFFSET = { -0x30 }, SCREEN_ID_OFFSET = { 0x14 }, PIPE_OFFSET = { 0xADC };
        int[] X_OFFSET = { 0x1C }, Y_OFFSET = { 0x28 }, RESPAWN_X_OFFSET = { 0xA80 }, RESPAWN_Y_OFFSET = { 0xA84 };
        int[] LIVES_OFFSET = { 0xA88 }, STARS_OFFSET = { 0xA8C }, TIME_OFFSET = { 0xA98 }, SCORE_OFFSET = { 0xA9C };
        int[] ANIMATION_OFFSET = { 0x50 }, POWERUP_OFFSET = { 0x54 }, PSPEED_OFFSET = { 0x88 }, FLIGHT_OFFSET = { 0x90 }, GROUNDED_FLAG_OFFSET = { 0x74 };
        int[] STAGE_ID_OFFSET = { 0x2C8, 0x108, 0x47 }; //the first value can change a lot from a PC to another...

        int storedX = 1, storedY = 1, storedStars = 0, storedTime = 500, storedScore = 0, storedScreenID = 0;
        float XF = 0, YF = 0, storedXF = 1, storedYF = 1; //float coordinates
        int X, Y, stars, time, score, powerup, screenID, pipe, state; //a thread will read various values from the game and store them in these ints here
        string stageID; //a thread will read the stage ID value and store it in this string here
        List<String> stageList = new List<String>();
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

            //SCREENS - this method kinda sucks but I couldn't come up with something different
            //dictScreen contains a mapping of screens/pipes per stage -> key is stage name and value is a list with ints
            //if list is null then the stage is only a single screen
            dictScreen.Add("0-1-2", null); dictScreen.Add("0-2-2", null); dictScreen.Add("1-2-1", null);
            dictScreen.Add("2-1-2", null); dictScreen.Add("2-2-2", null); dictScreen.Add("3-1-1", null);
            dictScreen.Add("3-2-2", null); dictScreen.Add("4-2-3", null); dictScreen.Add("5-2-1", null);
            dictScreen.Add("5-2-2", null); dictScreen.Add("6-1-3", null); dictScreen.Add("6-2-4", null);
            dictScreen.Add("7-1-1", null); dictScreen.Add("7-1-3", null); dictScreen.Add("7-2-3", null);

            //if position in that list is even then it's a screen ID, if it's odd then it's a the pipe number leading to that screen ID
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
            dictScreen.Add("4-2-1", new int[] {1,6}); dictScreen.Add("4-2-2", new int[] {0,1, 1,16});  dictScreen.Add("4-2-4", new int[] {0,15, 5,16});
            
            dictScreen.Add("5-1-1", new int[] {0,7}); dictScreen.Add("5-1-2", new int[] {0,1, 1,16});
            dictScreen.Add("5-1-3", new int[] {1,1, 2,2, 3,3, 4,4}); dictScreen.Add("5-1-4", new int[] {0,1, 1,16});
            dictScreen.Add("5-2-3", new int[] {1,1, 2,3, 3,4, 4,5, 5,6, 6,7, 7,8, 8,9, 9,10}); dictScreen.Add("5-2-4", new int[] {1,1, 2,5, 3,16});
            
            dictScreen.Add("6-1-1", new int[] {0,1}); dictScreen.Add("6-1-2", new int[] {0,1, 4,2, 2,3, 6,4, 3,5, 7,16}); dictScreen.Add("6-1-4", new int[] {2,1, 1,16});
            dictScreen.Add("6-2-1", new int[] {1,1, 2,2, 3,3, 4,4, 5,5, 6,6, 7,7, 8,8, 9,9}); dictScreen.Add("6-2-2", new int[] {2,1, 3,16}); dictScreen.Add("6-2-3", new int[] {0,1}); 
            
            dictScreen.Add("7-1-2", new int[] {1,1, 0,16}); dictScreen.Add("7-1-4", new int[] {0,1, 1,16});
            dictScreen.Add("7-2-1", new int[] {1,3}); dictScreen.Add("7-2-2", new int[] {2,1, 3,2, 5,3, 6,4, 0,15, 1,16}); dictScreen.Add("7-2-4", new int[] {1,1, 2,16});

            //starting threads
            new Thread(ReadValues) { IsBackground = true }.Start();
            new Thread(Freeze) { IsBackground = true }.Start();
            new Thread(Coordinates) { IsBackground = true }.Start();
            new Thread(PSpeed) { IsBackground = true }.Start();
            new Thread(TrackPowerup) { IsBackground = true }.Start();
            new Thread(TrackDeath) { IsBackground = true }.Start();
            new Thread(TrackPSpeedAfterJump) { IsBackground = true }.Start(); //keeping this as its own thing for accurate readings
            new Thread(EnableDisablePopulateControls) { IsBackground = true }.Start();
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

        private void Freeze()
        {
            bool flagPowerup = false;
            byte[] localPowerup = new byte[1]; //rereading them here for consistency

            while (true)
            {
                if (checkPSpeedFly.Checked)
                {
                    pm.Write(FIRST_OFFSET, PSPEED_OFFSET, BitConverter.GetBytes(120));
                    pm.Write(FIRST_OFFSET, FLIGHT_OFFSET, BitConverter.GetBytes(200));
                }

                if (checkSPREADMYWINGS.Checked)
                    pm.Write(FIRST_OFFSET, ANIMATION_OFFSET, BitConverter.GetBytes(8));

                if (checkPowerup.Checked)
                    if (!flagPowerup) { localPowerup = pm.Read(FIRST_OFFSET, POWERUP_OFFSET); flagPowerup = true; }
                    else pm.Write(FIRST_OFFSET, POWERUP_OFFSET, localPowerup);
                else flagPowerup = false;

                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread and because I access elements in the form
                    {
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
                Thread.Sleep(1);
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
                        if (XF != 0)
                            labelX.Text = "X: " + XF.ToString("0.000");
                        else
                            labelX.Text = "X: 0";

                        if (YF != 0)
                            labelY.Text = "Y: " + YF.ToString("0.000");
                        else
                            labelY.Text = "Y: 0";
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Caught exception: " + ex.Message);
                }
                Thread.Sleep(25);
            }
        }
        private void TrackDeath()
        {
            int sleep = 100;

            while (true)
            {
                if (checkRespawn.Checked)
                {
                    sleep = 1;
                    if (state == 4) //4 means DEATH
                    {
                        pm.Write(FIRST_OFFSET, X_OFFSET, BitConverter.GetBytes(storedX));
                        pm.Write(FIRST_OFFSET, Y_OFFSET, BitConverter.GetBytes(storedY));
                        pm.Write(FIRST_OFFSET, SCREEN_ID_OFFSET, BitConverter.GetBytes(screenID));
                    }
                }
                else
                    sleep = 100;
                Thread.Sleep(sleep);
            }
        }

        private void TrackPowerup()
        {
            while (true)
            {
                if (powerup == 0 && (X != 0 || Y != 0)) //when no powerup -> offer switching to miko
                {
                    buttonPowerup.Invoke((MethodInvoker)delegate () { buttonPowerup.Text = "Power-up: Switch to Miko"; });
                    buttonPowerup.BackColor = Color.FromArgb(255, 153, 153);
                    checkSPREADMYWINGS.Invoke((MethodInvoker)delegate () { checkSPREADMYWINGS.Enabled = true; });
                }
                else if (powerup == 1) //when miko -> offer switching to broom
                {
                    buttonPowerup.Invoke((MethodInvoker)delegate () { buttonPowerup.Text = "Power-up: Switch to Broom"; });
                    buttonPowerup.BackColor = Color.FromArgb(255, 209, 146);
                    checkSPREADMYWINGS.Invoke((MethodInvoker)delegate () { checkSPREADMYWINGS.Enabled = false; });
                }
                else if (powerup == 2) //when broom -> offer switching to marisa
                {
                    buttonPowerup.Invoke((MethodInvoker)delegate () { buttonPowerup.Text = "Power-up: Back to normal"; });
                    buttonPowerup.BackColor = Color.FromArgb(175, 175, 175);
                    checkSPREADMYWINGS.Invoke((MethodInvoker)delegate () { checkSPREADMYWINGS.Enabled = false; });
                }
                else
                {
                    buttonPowerup.Invoke((MethodInvoker)delegate () { buttonPowerup.Text = "Power-up: Back to normal"; });
                    buttonPowerup.BackColor = Color.FromArgb(153, 153, 153);
                    checkSPREADMYWINGS.Invoke((MethodInvoker)delegate () { checkSPREADMYWINGS.Enabled = false; });
                }
                Thread.Sleep(100);
            }
        }

        private void EnableDisablePopulateControls()
        {
            int oldScreenID = -1;
            bool buttonsActivated = true, inStage = false;

            while (true)
            {
                try
                {
                    Invoke((MethodInvoker)delegate //using this because thread
                    {
                        //if marisa is in a stage (both her coordinates different than 0) and the screen is different than the old one
                        if (X != 0 || Y != 0)
                        {
                            if (stageList.Contains(stageID))
                            {
                                if (screenID != oldScreenID)
                                {
                                    Thread.Sleep(1000); //for safety - give the game some time to update its values
                                    if (screenID < 50 && screenID >= 0)
                                    {
                                        oldScreenID = screenID;
                                        buttonRescan.BackColor = SystemColors.Control;
                                        buttonDelete.Enabled = true; buttonSave.Enabled = true;
                                        checkSave.Enabled = true; comboSaves.Enabled = true;
                                        buttonSave.BackColor = Color.FromArgb(176, 196, 222);
                                        buttonDelete.BackColor = Color.FromArgb(176, 196, 222);
                                        buttonPreviousScreen.BackColor = Color.FromArgb(176, 196, 222);
                                        buttonNextScreen.BackColor = Color.FromArgb(176, 196, 222);
                                        if (dictScreen[stageID] != null)
                                        {
                                            buttonPreviousScreen.Enabled = true;
                                            buttonNextScreen.Enabled = true;
                                        }
                                        UpdateComboSaveStates();
                                        StoreCoordinates();
                                        comboSaves.SelectedIndex = -1;
                                        inStage = true;
                                        labelStatus.ForeColor = Color.Gold;
                                        labelStatus.Text = "Marisa is in stage " + stageID + " screen " + screenID;
                                    }
                                }
                                if (pipe != 0)
                                    labelStatus.Text = "Marisa is in stage " + stageID + " screen " + screenID + " entering pipe " + pipe;
                                else
                                    labelStatus.Text = "Marisa is in stage " + stageID + " screen " + screenID;
                            }
                            else
                            {
                                if (!inStage)
                                {
                                    buttonSave.BackColor = Color.FromArgb(153, 153, 153);
                                    buttonDelete.BackColor = Color.FromArgb(153, 153, 153);
                                    buttonPreviousScreen.BackColor = Color.FromArgb(153, 153, 153);
                                    buttonNextScreen.BackColor = Color.FromArgb(153, 153, 153);
                                    labelStatus.ForeColor = Color.FromArgb(255, 82, 82);
                                    labelStatus.Text = "Unable to detect where Marisa is. Click the 'Rescan' button to try and fix the issue...";
                                    buttonRescan.BackColor = Color.LightGreen;
                                }
                            }
                            if (!buttonsActivated)
                            {
                                buttonStore.Enabled = true; buttonLoad.Enabled = true; buttonGo.Enabled = true;
                                textX.Enabled = true; textY.Enabled = true; buttonPowerup.Enabled = true;
                                checkLives.Enabled = true; checkPowerup.Enabled = true; checkTime.Enabled = true;
                                checkScore.Enabled = true; checkStars.Enabled = true; checkPSpeedFly.Enabled = true;
                                buttonShmup.Enabled = true;
                                numericStars.Enabled = true; numericTime.Enabled = true; numericScore.Enabled = true;
                                checkRespawn.Enabled = true; numericLives.Enabled = true; buttonLoad.BackColor = Color.FromArgb(176, 196, 222);
                                buttonGo.BackColor = Color.FromArgb(176, 196, 222); buttonStore.BackColor = Color.FromArgb(176, 196, 222);
                                buttonsActivated = true;
                            }
                        }
                        else //if on title screen or world map
                        {
                            inStage = false;
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
                                buttonDelete.Enabled = false; buttonSave.Enabled = false; checkSave.Enabled = false;
                                buttonPreviousScreen.Enabled = false; buttonNextScreen.Enabled = false;
                                buttonStore.Enabled = false; buttonLoad.Enabled = false; comboSaves.Enabled = false;
                                buttonGo.Enabled = false; textX.Enabled = false; textY.Enabled = false;
                                checkLives.Enabled = false; checkPowerup.Enabled = false; checkTime.Enabled = false;
                                checkScore.Enabled = false; checkStars.Enabled = false; checkPSpeedFly.Enabled = false;
                                numericStars.Enabled = false; numericTime.Enabled = false; numericScore.Enabled = false;
                                checkPowerup.Checked = false; checkRespawn.Enabled = false; numericLives.Enabled = false;
                                checkLives.Checked = false; checkStars.Checked = false; checkSave.Checked = false;
                                checkTime.Checked = false; checkScore.Checked = false; checkPSpeedFly.Checked = false;
                                buttonPowerup.Enabled = false; comboSaves.Items.Clear();
                                buttonShmup.Enabled = false;
                                labelStoredX.Text = "X:"; labelStoredY.Text = "Y:"; textX.Text = ""; textY.Text = "";
                                labelStoredStars.Text = "Stars:"; labelStoredTime.Text = "Time:"; labelStoredScore.Text = "Score:";
                                buttonLoad.BackColor = Color.FromArgb(153, 153, 153); buttonSave.BackColor = Color.FromArgb(153, 153, 153);
                                buttonDelete.BackColor = Color.FromArgb(153, 153, 153); buttonStore.BackColor = Color.FromArgb(153, 153, 153);
                                buttonPreviousScreen.BackColor = Color.FromArgb(153, 153, 153); buttonNextScreen.BackColor = Color.FromArgb(153, 153, 153);
                                buttonGo.BackColor = Color.FromArgb(153, 153, 153);
                                oldScreenID = -1; buttonsActivated = false;
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Caught exception: " + ex.Message);
                }
                Thread.Sleep(5);
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
                                {
                                    if (line.EndsWith(screenID.ToString()))
                                        comboSaves.Items.Add(line);
                                }

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

            if (split2.Length > 3)
            {
                storedStars = int.Parse(split2[2]); //stars
                storedTime = int.Parse(split2[3]); //time
                storedScore = int.Parse(split2[4]); //score
            }
            else //if none of stars/time/score was stored, just use the current ones
            {
                storedStars = stars; storedTime = time; storedScore = score;
            }
            labelStoredStars.Text = "Stars: " + storedStars;
            labelStoredTime.Text = "Time: " + storedTime;
            labelStoredScore.Text = "Score: " + storedScore;

            LoadStoredValues();
        }
        #endregion

        #region hotkeys
        public void WriteDefaultHotkeyConfig()
        {
            TextWriter writer = new StreamWriter(configpath + hotkeyfilename);
            writer.WriteLine("D7\nAlt\nD6\nAlt\nD5\nAlt\nD4\nAlt\nD3\nAlt\nD2\nAlt\nD1\nAlt");
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
                using (StreamReader sr = File.OpenText(configpath + hotkeyfilename))
                {
                    for (int i = 1, m = 0, k = 0; i < numberHotkeys * 2 + 1; i++)
                    {
                        if (i % 2 != 0) //if the line number is odd then it's a modifier
                        {
                            KeysConverter kc = new KeysConverter();
                            keys[k] = (Keys)kc.ConvertFromString(sr.ReadLine());
                            k++;
                        }
                        else //else if it's an even number then it's a hotkey
                        {
                            mods[m] = sr.ReadLine();
                            m++;
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
        }
        
        private void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            //these are in reverse - when adding a new hotkey, push everything down and add the new one at the top [0]
            if (mods[0] == "Alt" && e.Key == keys[0] || mods[0] == "Control" && e.Key == keys[0] || mods[0] == "Shift" && e.Key == keys[0] || e.Key == keys[0])
                buttonNextScreen_Click(null, null);
            if (mods[1] == "Alt" && e.Key == keys[1] || mods[1] == "Control" && e.Key == keys[1] || mods[1] == "Shift" && e.Key == keys[1] || e.Key == keys[1])
                buttonPreviousScreen_Click(null, null);
            if (mods[2] == "Alt" && e.Key == keys[2] || mods[2] == "Control" && e.Key == keys[2] || mods[2] == "Shift" && e.Key == keys[2] || e.Key == keys[2])
                LoadNextSaveState();
            if (mods[3] == "Alt" && e.Key == keys[3] || mods[3] == "Control" && e.Key == keys[3] || mods[3] == "Shift" && e.Key == keys[3] || e.Key == keys[3])
                SetNumericValues();
            if (mods[4] == "Alt" && e.Key == keys[4] || mods[4] == "Control" && e.Key == keys[4] || mods[4] == "Shift" && e.Key == keys[4] || e.Key == keys[4])
                LoadStoredValues();
            if (mods[5] == "Alt" && e.Key == keys[5] || mods[5] == "Control" && e.Key == keys[5] || mods[5] == "Shift" && e.Key == keys[5] || e.Key == keys[5])
            { StoreCoordinates(); comboSaves.SelectedIndex = -1; }
            if (mods[6] == "Alt" && e.Key == keys[6] || mods[6] == "Control" && e.Key == keys[6] || mods[6] == "Shift" && e.Key == keys[6] || e.Key == keys[6])
                ChangePowerup();
        }
        #endregion

        #region actions
        private void StoreCoordinates()
        {
            byte[] xPos = pm.Read(FIRST_OFFSET, X_OFFSET); //read x speed value
            byte[] yPos = pm.Read(FIRST_OFFSET, Y_OFFSET); //read y speed value

            storedX = BitConverter.ToInt32(xPos, 0); //convert to int globally
            storedY = BitConverter.ToInt32(yPos, 0); //convert to int globally
            storedXF = BitConverter.ToSingle(xPos, 0); //convert to float globally
            storedYF = BitConverter.ToSingle(yPos, 0); //convert to float globally
            storedStars = stars; storedTime = time; storedScore = score;

            labelStoredX.Text = "X: " + XF.ToString("0.000");
            labelStoredY.Text = "Y: " + YF.ToString("0.000");
            labelStoredStars.Text = "Stars: " + storedStars;
            labelStoredTime.Text = "Time: " + storedTime;
            labelStoredScore.Text = "Score: " + storedScore;
        }

        private void LoadStoredValues()
        {
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

        private void SetNumericValues() //sets the values entered in the Stars, Time and Score numeric text boxes
        {
            int desiredStars = int.Parse(numericStars.Text);
            int desiredTime = int.Parse(numericTime.Text);
            int desiredScore = int.Parse(numericScore.Text);

            pm.Write(FIRST_OFFSET, STARS_OFFSET, BitConverter.GetBytes(desiredStars));
            pm.Write(FIRST_OFFSET, TIME_OFFSET, BitConverter.GetBytes(desiredTime));
            pm.Write(FIRST_OFFSET, SCORE_OFFSET, BitConverter.GetBytes(desiredScore));
        }
        #endregion

        #region buttons
        private void buttonHotkeys_Click(object sender, EventArgs e)
        {
            HotkeyDialog hd = new HotkeyDialog();
            hd.ShowDialog();
            LoadHotkeys();
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            if (textX.Text.Length > 0 && textY.Text.Length > 0)
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
            StoreCoordinates();
            comboSaves.SelectedIndex = -1;
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            LoadStoredValues();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (stageList.Contains(stageID))
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

        private void buttonRescan_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Go anywhere on the world map or in a stage, then click OK.",
                            "Fixing the tool",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);

            int[] LOCAL_STAGE_ID_OFFSET = { 0x0, 0x108, 0x47 };
            int increment = 0x0;
            while (true)
            {
                LOCAL_STAGE_ID_OFFSET[0] = increment;
                byte[] buffer = pm.Read(FIRST_OFFSET_STAGE_ID, LOCAL_STAGE_ID_OFFSET);
                string localStageID = Encoding.Default.GetString(buffer);
                if (stageList.Contains(localStageID))
                {
                    MessageBox.Show("Successfully rescanned with the following first offset: " + increment.ToString("X3"),
                                    "KoakumaYay",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    STAGE_ID_OFFSET = LOCAL_STAGE_ID_OFFSET;
                    buttonRescan.BackColor = SystemColors.Control;
                    break;
                }
                if (increment > 0x10000)
                {
                    MessageBox.Show("Unable to find stage ID... Save states won't be usable.",
                                    "No address found",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    break;
                }
                increment += 0x1;
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
                        if (i % 2 == 0) //i is an even number -> dealing with a screen
                        {
                            if (screen_pipe == screenID) //if marisa's current screen is in the list
                            {
                                if (i+3 <= dictScreen[stageID].Length) //make sure we don't overshoot the array positions
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
                    //when current screen isn't in the list, marisa is in the spawn screen
                    //and when marisa is in the last possible screen, this bool will remain false, so this loops back to the first possible screen
                    if (!currentScreenFound)
                        //send her to the first screen possible, aka first pipe which is always in position [1]
                        pm.Write(FIRST_OFFSET, PIPE_OFFSET, BitConverter.GetBytes(dictScreen[stageID][1]));
                }
            }
        }

        private void buttonShmup_Click(object sender, EventArgs e)
        {
            pm.Write(FIRST_OFFSET, POWERUP_OFFSET, BitConverter.GetBytes(9));
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }
        #endregion
    }
}
