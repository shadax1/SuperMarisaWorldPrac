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
        static int numberHotkeys = 4;
        Keys[] keys = new Keys[numberHotkeys];
        string[] mods = new string[numberHotkeys];
        #endregion

        #region memory stuff
        static ProcessMemory pm = new ProcessMemory();
        int[] FIRST_OFFSET = { 0x155438 }; //offset to be added to "SuperMarisaWorld.exe" when reading/writing
        int[] SCREEN_ID_OFFSET = { 0x14 };
        int[] X_OFFSET = { 0x1C }, Y_OFFSET = { 0x28 };
        int[] LIVES_OFFSET = { 0xA88 }, STARS_OFFSET = { 0xA8C }, TIME_OFFSET = { 0xA98 }, SCORE_OFFSET = { 0xA9C };
        int[] POWERUP_OFFSET = { 0x54 }, PSPEED_OFFSET = { 0x88 }, FLIGHT_OFFSET = { 0x90 }, GROUNDED_FLAG_OFFSET = { 0x74 };

        int[] FIRST_OFFSET_STAGE_ID = { 0x15542C }; //offset to be added to "SuperMarisaWorld.exe" when reading/writing
        int[] STAGE_ID_OFFSET = { 0x2C8, 0x108, 0x47 }; //the first value can change a lot from a PC to another...

        int storedX = 1, storedY = 1, storedStars = 0, storedTime = 500, storedScore = 0;
        float XF = 0, YF = 0; //float coordinates
        int X, Y, stars, time, score, powerup, screenID; //a thread will read various values from the game and store them in these ints here
        string stageID; //a thread will read the stage ID value and store it in this string here
        List<String> stageList = new List<String>();
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

            //starting threads
            new Thread(ReadValues) { IsBackground = true }.Start();
            new Thread(Freeze) { IsBackground = true }.Start();
            new Thread(Coordinates) { IsBackground = true }.Start();
            new Thread(PSpeed) { IsBackground = true }.Start();
            new Thread(TrackPSpeedAfterJump) { IsBackground = true }.Start();
            new Thread(TrackPowerup) { IsBackground = true }.Start();
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

                buffer = pm.Read(FIRST_OFFSET, SCREEN_ID_OFFSET);
                screenID = BitConverter.ToInt32(buffer, 0);
                buffer = pm.Read(FIRST_OFFSET_STAGE_ID, STAGE_ID_OFFSET);
                stageID = Encoding.Default.GetString(buffer);
                Thread.Sleep(1);
            }
        }

        private void Freeze()
        {
            bool flagLives = false;
            bool flagPowerup = false;
            byte[] localLives = new byte[1]; //rereading them here for consistency
            byte[] localPowerup = new byte[1]; //rereading them here for consistency

            while (true)
            {
                if (checkSpeedFly.Checked)
                {
                    pm.Write(FIRST_OFFSET, PSPEED_OFFSET, BitConverter.GetBytes(120));
                    pm.Write(FIRST_OFFSET, FLIGHT_OFFSET, BitConverter.GetBytes(200));
                }

                if (checkLives.Checked)
                    if (!flagLives) { localLives = pm.Read(FIRST_OFFSET, LIVES_OFFSET); flagLives = true; }
                    else pm.Write(FIRST_OFFSET, LIVES_OFFSET, localLives);
                else flagLives = false;

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
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Caught exception: " + ex.Message);
                }
                Thread.Sleep(10);
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
                            if (!checkSpeedFly.Checked)
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

        private void TrackPowerup()
        {
            while (true)
            {
                if (powerup == 0 && (X != 0 || Y != 0)) //when no powerup -> offer switching to miko
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
                    buttonPowerup.Invoke((MethodInvoker)delegate () { buttonPowerup.Text = "Power-up"; });
                    buttonPowerup.BackColor = Color.FromArgb(153, 153, 153);
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
                                        buttonDelete.Enabled = true; buttonSave.Enabled = true;
                                        checkSave.Enabled = true; comboSaves.Enabled = true;
                                        buttonRescan.BackColor = SystemColors.Control;
                                        buttonSave.BackColor = Color.FromArgb(176, 196, 222);
                                        buttonDelete.BackColor = Color.FromArgb(176, 196, 222);
                                        UpdateComboSaveStates();
                                        StoreCoordinates();
                                        comboSaves.SelectedIndex = -1;
                                        inStage = true;
                                        labelStatus.ForeColor = Color.Gold;
                                        labelStatus.Text = "Marisa is in stage " + stageID + " screen " + screenID;
                                    }
                                }
                            }
                            else
                            {
                                if (!inStage)
                                {
                                    labelStatus.ForeColor = Color.FromArgb(255, 82, 82);
                                    labelStatus.Text = "Unable to detect where Marisa is. Click the 'Rescan' button to try and fix the issue...";
                                    buttonRescan.BackColor = Color.LightSteelBlue;
                                }
                            }
                            if (!buttonsActivated)
                            {
                                buttonStore.Enabled = true; buttonLoad.Enabled = true; buttonGo.Enabled = true;
                                textX.Enabled = true; textY.Enabled = true; buttonPowerup.Enabled = true;
                                checkLives.Enabled = true; checkPowerup.Enabled = true; checkTime.Enabled = true;
                                checkScore.Enabled = true; checkStars.Enabled = true; checkSpeedFly.Enabled = true;
                                numericStars.Enabled = true; numericTime.Enabled = true; numericScore.Enabled = true;
                                buttonGo.BackColor = Color.FromArgb(176, 196, 222); buttonStore.BackColor = Color.FromArgb(176, 196, 222);
                                buttonLoad.BackColor = Color.FromArgb(176, 196, 222);
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
                                buttonStore.Enabled = false; buttonLoad.Enabled = false; comboSaves.Enabled = false;
                                buttonGo.Enabled = false; textX.Enabled = false; textY.Enabled = false;
                                checkLives.Enabled = false; checkPowerup.Enabled = false; checkTime.Enabled = false;
                                checkScore.Enabled = false; checkStars.Enabled = false; checkSpeedFly.Enabled = false;
                                numericStars.Enabled = false; numericTime.Enabled = false; numericScore.Enabled = false;
                                buttonPowerup.Enabled = false; comboSaves.Items.Clear(); checkSave.Checked = false;
                                labelStoredX.Text = "X:"; labelStoredY.Text = "Y:"; textX.Text = ""; textY.Text = "";
                                labelStoredStars.Text = "Stars:"; labelStoredTime.Text = "Time:"; labelStoredScore.Text = "Score:";
                                buttonGo.BackColor = Color.FromArgb(153, 153, 153); buttonSave.BackColor = Color.FromArgb(153, 153, 153);
                                buttonDelete.BackColor = Color.FromArgb(153, 153, 153); buttonStore.BackColor = Color.FromArgb(153, 153, 153);
                                buttonLoad.BackColor = Color.FromArgb(153, 153, 153);
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
            writer.WriteLine("D4\nAlt\nD3\nAlt\nD2\nAlt\nD1\nAlt");
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
                SetNumericValues();
            if (mods[1] == "Alt" && e.Key == keys[1] || mods[1] == "Control" && e.Key == keys[1] || mods[1] == "Shift" && e.Key == keys[1] || e.Key == keys[1])
                LoadStoredValues();
            if (mods[2] == "Alt" && e.Key == keys[2] || mods[2] == "Control" && e.Key == keys[2] || mods[2] == "Shift" && e.Key == keys[2] || e.Key == keys[2])
            { StoreCoordinates(); comboSaves.SelectedIndex = -1; }
            if (mods[3] == "Alt" && e.Key == keys[3] || mods[3] == "Control" && e.Key == keys[3] || mods[3] == "Shift" && e.Key == keys[3] || e.Key == keys[3])
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
                                lines.Insert(lineNumber - 1, ssname.name + " | " + XF + "," + YF + "," + stars + "," + time + "," + score + "," + screenID);
                            else
                                lines.Insert(lineNumber - 1, ssname.name + " | " + XF + "," + YF + "," + screenID);
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

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }
        #endregion
    }
}
