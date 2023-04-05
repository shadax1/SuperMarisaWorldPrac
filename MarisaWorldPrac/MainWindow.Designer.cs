namespace SuperMarisaWorldPrac
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.labelX = new System.Windows.Forms.Label();
            this.labelY = new System.Windows.Forms.Label();
            this.labelP = new System.Windows.Forms.Label();
            this.pSpeedBar = new System.Windows.Forms.ProgressBar();
            this.groupCoordinates = new System.Windows.Forms.GroupBox();
            this.labelStoredY = new System.Windows.Forms.Label();
            this.labelStoredX = new System.Windows.Forms.Label();
            this.labelStoredScore = new System.Windows.Forms.Label();
            this.labelStoredTime = new System.Windows.Forms.Label();
            this.labelStoredStars = new System.Windows.Forms.Label();
            this.buttonLoad = new System.Windows.Forms.Button();
            this.buttonStore = new System.Windows.Forms.Button();
            this.buttonGo = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textY = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textX = new System.Windows.Forms.TextBox();
            this.buttonNextScreen = new System.Windows.Forms.Button();
            this.buttonPreviousScreen = new System.Windows.Forms.Button();
            this.checkSave = new System.Windows.Forms.CheckBox();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.comboSaves = new System.Windows.Forms.ComboBox();
            this.groupPspeed = new System.Windows.Forms.GroupBox();
            this.flightBar = new System.Windows.Forms.ProgressBar();
            this.labelJumpP = new System.Windows.Forms.Label();
            this.buttonPowerup = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.checkStars = new System.Windows.Forms.CheckBox();
            this.checkTime = new System.Windows.Forms.CheckBox();
            this.checkLives = new System.Windows.Forms.CheckBox();
            this.checkPowerup = new System.Windows.Forms.CheckBox();
            this.checkPSpeedFly = new System.Windows.Forms.CheckBox();
            this.checkScore = new System.Windows.Forms.CheckBox();
            this.numericStars = new System.Windows.Forms.NumericUpDown();
            this.numericScore = new System.Windows.Forms.NumericUpDown();
            this.numericTime = new System.Windows.Forms.NumericUpDown();
            this.numericLives = new System.Windows.Forms.NumericUpDown();
            this.checkRespawn = new System.Windows.Forms.CheckBox();
            this.checkSPREADMYWINGS = new System.Windows.Forms.CheckBox();
            this.groupFreeze = new System.Windows.Forms.GroupBox();
            this.comboScoreRandomizer = new System.Windows.Forms.ComboBox();
            this.checkScoreRandomizer = new System.Windows.Forms.CheckBox();
            this.checkIframes = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpAboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupSaves = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.labelTimerStage = new System.Windows.Forms.Label();
            this.labelTimerSaveState = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupCoordinates.SuspendLayout();
            this.groupPspeed.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericStars)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericScore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLives)).BeginInit();
            this.groupFreeze.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.groupSaves.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelX.ForeColor = System.Drawing.Color.Blue;
            this.labelX.Location = new System.Drawing.Point(50, 25);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(30, 13);
            this.labelX.TabIndex = 5;
            this.labelX.Text = "X: 0";
            // 
            // labelY
            // 
            this.labelY.AutoSize = true;
            this.labelY.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelY.ForeColor = System.Drawing.Color.Red;
            this.labelY.Location = new System.Drawing.Point(127, 25);
            this.labelY.Name = "labelY";
            this.labelY.Size = new System.Drawing.Size(30, 13);
            this.labelY.TabIndex = 6;
            this.labelY.Text = "Y: 0";
            // 
            // labelP
            // 
            this.labelP.AutoSize = true;
            this.labelP.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelP.Location = new System.Drawing.Point(9, 16);
            this.labelP.Name = "labelP";
            this.labelP.Size = new System.Drawing.Size(49, 16);
            this.labelP.TabIndex = 9;
            this.labelP.Text = "Current";
            // 
            // pSpeedBar
            // 
            this.pSpeedBar.Location = new System.Drawing.Point(6, 35);
            this.pSpeedBar.Maximum = 120;
            this.pSpeedBar.Name = "pSpeedBar";
            this.pSpeedBar.Size = new System.Drawing.Size(461, 15);
            this.pSpeedBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pSpeedBar.TabIndex = 10;
            // 
            // groupCoordinates
            // 
            this.groupCoordinates.Controls.Add(this.labelStoredY);
            this.groupCoordinates.Controls.Add(this.labelStoredX);
            this.groupCoordinates.Controls.Add(this.labelStoredScore);
            this.groupCoordinates.Controls.Add(this.labelStoredTime);
            this.groupCoordinates.Controls.Add(this.labelStoredStars);
            this.groupCoordinates.Controls.Add(this.buttonLoad);
            this.groupCoordinates.Controls.Add(this.buttonStore);
            this.groupCoordinates.Controls.Add(this.buttonGo);
            this.groupCoordinates.Controls.Add(this.label2);
            this.groupCoordinates.Controls.Add(this.label5);
            this.groupCoordinates.Controls.Add(this.label1);
            this.groupCoordinates.Controls.Add(this.labelY);
            this.groupCoordinates.Controls.Add(this.label4);
            this.groupCoordinates.Controls.Add(this.labelX);
            this.groupCoordinates.Controls.Add(this.textY);
            this.groupCoordinates.Controls.Add(this.label3);
            this.groupCoordinates.Controls.Add(this.textX);
            this.groupCoordinates.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupCoordinates.Location = new System.Drawing.Point(12, 27);
            this.groupCoordinates.Name = "groupCoordinates";
            this.groupCoordinates.Size = new System.Drawing.Size(207, 178);
            this.groupCoordinates.TabIndex = 16;
            this.groupCoordinates.TabStop = false;
            this.groupCoordinates.Text = "Coordinates";
            // 
            // labelStoredY
            // 
            this.labelStoredY.AutoSize = true;
            this.labelStoredY.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F);
            this.labelStoredY.ForeColor = System.Drawing.Color.Red;
            this.labelStoredY.Location = new System.Drawing.Point(114, 64);
            this.labelStoredY.Name = "labelStoredY";
            this.labelStoredY.Size = new System.Drawing.Size(26, 13);
            this.labelStoredY.TabIndex = 28;
            this.labelStoredY.Text = "Y: 0";
            // 
            // labelStoredX
            // 
            this.labelStoredX.AutoSize = true;
            this.labelStoredX.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F);
            this.labelStoredX.ForeColor = System.Drawing.Color.Blue;
            this.labelStoredX.Location = new System.Drawing.Point(114, 51);
            this.labelStoredX.Name = "labelStoredX";
            this.labelStoredX.Size = new System.Drawing.Size(26, 13);
            this.labelStoredX.TabIndex = 27;
            this.labelStoredX.Text = "X: 0";
            // 
            // labelStoredScore
            // 
            this.labelStoredScore.AutoSize = true;
            this.labelStoredScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F);
            this.labelStoredScore.ForeColor = System.Drawing.Color.DarkGreen;
            this.labelStoredScore.Location = new System.Drawing.Point(50, 77);
            this.labelStoredScore.Name = "labelStoredScore";
            this.labelStoredScore.Size = new System.Drawing.Size(37, 13);
            this.labelStoredScore.TabIndex = 26;
            this.labelStoredScore.Text = "Score:";
            // 
            // labelStoredTime
            // 
            this.labelStoredTime.AutoSize = true;
            this.labelStoredTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F);
            this.labelStoredTime.ForeColor = System.Drawing.Color.DarkGreen;
            this.labelStoredTime.Location = new System.Drawing.Point(50, 64);
            this.labelStoredTime.Name = "labelStoredTime";
            this.labelStoredTime.Size = new System.Drawing.Size(31, 13);
            this.labelStoredTime.TabIndex = 25;
            this.labelStoredTime.Text = "Time:";
            // 
            // labelStoredStars
            // 
            this.labelStoredStars.AutoSize = true;
            this.labelStoredStars.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F);
            this.labelStoredStars.ForeColor = System.Drawing.Color.DarkGreen;
            this.labelStoredStars.Location = new System.Drawing.Point(50, 51);
            this.labelStoredStars.Name = "labelStoredStars";
            this.labelStoredStars.Size = new System.Drawing.Size(34, 13);
            this.labelStoredStars.TabIndex = 24;
            this.labelStoredStars.Text = "Stars:";
            // 
            // buttonLoad
            // 
            this.buttonLoad.BackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonLoad.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonLoad.Location = new System.Drawing.Point(106, 100);
            this.buttonLoad.Name = "buttonLoad";
            this.buttonLoad.Size = new System.Drawing.Size(93, 19);
            this.buttonLoad.TabIndex = 18;
            this.buttonLoad.Text = "Load";
            this.buttonLoad.UseVisualStyleBackColor = false;
            this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // buttonStore
            // 
            this.buttonStore.BackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonStore.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStore.Location = new System.Drawing.Point(6, 100);
            this.buttonStore.Name = "buttonStore";
            this.buttonStore.Size = new System.Drawing.Size(93, 19);
            this.buttonStore.TabIndex = 17;
            this.buttonStore.Text = "Store";
            this.buttonStore.UseVisualStyleBackColor = false;
            this.buttonStore.Click += new System.EventHandler(this.buttonStore_Click);
            // 
            // buttonGo
            // 
            this.buttonGo.BackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonGo.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGo.Location = new System.Drawing.Point(6, 153);
            this.buttonGo.Name = "buttonGo";
            this.buttonGo.Size = new System.Drawing.Size(193, 19);
            this.buttonGo.TabIndex = 16;
            this.buttonGo.Text = "Go!!";
            this.buttonGo.UseVisualStyleBackColor = false;
            this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(7, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Stored:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Blue;
            this.label5.Location = new System.Drawing.Point(50, 130);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(19, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "X:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(7, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Coord:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(127, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Y:";
            // 
            // textY
            // 
            this.textY.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textY.Location = new System.Drawing.Point(152, 127);
            this.textY.Name = "textY";
            this.textY.Size = new System.Drawing.Size(41, 20);
            this.textY.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(6, 130);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Custom:";
            // 
            // textX
            // 
            this.textX.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textX.Location = new System.Drawing.Point(75, 127);
            this.textX.Name = "textX";
            this.textX.Size = new System.Drawing.Size(41, 20);
            this.textX.TabIndex = 14;
            // 
            // buttonNextScreen
            // 
            this.buttonNextScreen.BackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonNextScreen.Enabled = false;
            this.buttonNextScreen.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonNextScreen.Location = new System.Drawing.Point(106, 96);
            this.buttonNextScreen.Name = "buttonNextScreen";
            this.buttonNextScreen.Size = new System.Drawing.Size(93, 19);
            this.buttonNextScreen.TabIndex = 33;
            this.buttonNextScreen.Text = "Next screen";
            this.buttonNextScreen.UseVisualStyleBackColor = false;
            this.buttonNextScreen.Click += new System.EventHandler(this.buttonNextScreen_Click);
            // 
            // buttonPreviousScreen
            // 
            this.buttonPreviousScreen.BackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonPreviousScreen.Enabled = false;
            this.buttonPreviousScreen.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPreviousScreen.Location = new System.Drawing.Point(6, 96);
            this.buttonPreviousScreen.Name = "buttonPreviousScreen";
            this.buttonPreviousScreen.Size = new System.Drawing.Size(93, 19);
            this.buttonPreviousScreen.TabIndex = 32;
            this.buttonPreviousScreen.Text = "Previous screen";
            this.buttonPreviousScreen.UseVisualStyleBackColor = false;
            this.buttonPreviousScreen.Click += new System.EventHandler(this.buttonPreviousScreen_Click);
            // 
            // checkSave
            // 
            this.checkSave.AutoSize = true;
            this.checkSave.Enabled = false;
            this.checkSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.75F);
            this.checkSave.Location = new System.Drawing.Point(8, 46);
            this.checkSave.Name = "checkSave";
            this.checkSave.Size = new System.Drawing.Size(169, 17);
            this.checkSave.TabIndex = 22;
            this.checkSave.Text = "Include Stars, Time and Score";
            this.checkSave.UseVisualStyleBackColor = true;
            // 
            // buttonDelete
            // 
            this.buttonDelete.BackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonDelete.Enabled = false;
            this.buttonDelete.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDelete.Location = new System.Drawing.Point(106, 21);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(93, 19);
            this.buttonDelete.TabIndex = 21;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = false;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.BackColor = System.Drawing.Color.LightSteelBlue;
            this.buttonSave.Enabled = false;
            this.buttonSave.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSave.Location = new System.Drawing.Point(6, 21);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(93, 19);
            this.buttonSave.TabIndex = 20;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = false;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // comboSaves
            // 
            this.comboSaves.DisplayMember = "(none)";
            this.comboSaves.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSaves.Enabled = false;
            this.comboSaves.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboSaves.FormattingEnabled = true;
            this.comboSaves.Location = new System.Drawing.Point(6, 69);
            this.comboSaves.Name = "comboSaves";
            this.comboSaves.Size = new System.Drawing.Size(193, 21);
            this.comboSaves.TabIndex = 19;
            this.comboSaves.SelectionChangeCommitted += new System.EventHandler(this.comboSaves_SelectionChangeCommitted);
            // 
            // groupPspeed
            // 
            this.groupPspeed.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.groupPspeed.Controls.Add(this.labelP);
            this.groupPspeed.Controls.Add(this.flightBar);
            this.groupPspeed.Controls.Add(this.labelJumpP);
            this.groupPspeed.Controls.Add(this.pSpeedBar);
            this.groupPspeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupPspeed.Location = new System.Drawing.Point(12, 348);
            this.groupPspeed.Name = "groupPspeed";
            this.groupPspeed.Size = new System.Drawing.Size(473, 75);
            this.groupPspeed.TabIndex = 16;
            this.groupPspeed.TabStop = false;
            this.groupPspeed.Text = "P-speed";
            // 
            // flightBar
            // 
            this.flightBar.Location = new System.Drawing.Point(6, 54);
            this.flightBar.Maximum = 180;
            this.flightBar.Name = "flightBar";
            this.flightBar.Size = new System.Drawing.Size(461, 15);
            this.flightBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.flightBar.TabIndex = 12;
            // 
            // labelJumpP
            // 
            this.labelJumpP.AutoSize = true;
            this.labelJumpP.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelJumpP.Location = new System.Drawing.Point(333, 16);
            this.labelJumpP.Name = "labelJumpP";
            this.labelJumpP.Size = new System.Drawing.Size(76, 16);
            this.labelJumpP.TabIndex = 11;
            this.labelJumpP.Text = "Jumped at: ";
            // 
            // buttonPowerup
            // 
            this.buttonPowerup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPowerup.Location = new System.Drawing.Point(6, 216);
            this.buttonPowerup.Name = "buttonPowerup";
            this.buttonPowerup.Size = new System.Drawing.Size(248, 43);
            this.buttonPowerup.TabIndex = 17;
            this.buttonPowerup.Text = "Power-up";
            this.buttonPowerup.UseVisualStyleBackColor = true;
            this.buttonPowerup.Click += new System.EventHandler(this.buttonPowerup_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.BackColor = System.Drawing.SystemColors.ControlText;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 426);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(493, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 18;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // labelStatus
            // 
            this.labelStatus.ForeColor = System.Drawing.SystemColors.Control;
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(317, 17);
            this.labelStatus.Text = "If you see this, there is a problem. Click the \'Rescan\' button";
            // 
            // checkStars
            // 
            this.checkStars.AutoSize = true;
            this.checkStars.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkStars.Location = new System.Drawing.Point(62, 23);
            this.checkStars.Name = "checkStars";
            this.checkStars.Size = new System.Drawing.Size(50, 17);
            this.checkStars.TabIndex = 12;
            this.checkStars.Text = "Stars";
            this.checkStars.UseVisualStyleBackColor = true;
            // 
            // checkTime
            // 
            this.checkTime.AutoSize = true;
            this.checkTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkTime.Location = new System.Drawing.Point(118, 23);
            this.checkTime.Name = "checkTime";
            this.checkTime.Size = new System.Drawing.Size(49, 17);
            this.checkTime.TabIndex = 11;
            this.checkTime.Text = "Time";
            this.checkTime.UseVisualStyleBackColor = true;
            // 
            // checkLives
            // 
            this.checkLives.AutoSize = true;
            this.checkLives.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkLives.Location = new System.Drawing.Point(6, 23);
            this.checkLives.Name = "checkLives";
            this.checkLives.Size = new System.Drawing.Size(51, 17);
            this.checkLives.TabIndex = 2;
            this.checkLives.Text = "Lives";
            this.checkLives.UseVisualStyleBackColor = true;
            // 
            // checkPowerup
            // 
            this.checkPowerup.AutoSize = true;
            this.checkPowerup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkPowerup.Location = new System.Drawing.Point(6, 78);
            this.checkPowerup.Name = "checkPowerup";
            this.checkPowerup.Size = new System.Drawing.Size(71, 17);
            this.checkPowerup.TabIndex = 13;
            this.checkPowerup.Text = "Power-up";
            this.checkPowerup.UseVisualStyleBackColor = true;
            // 
            // checkPSpeedFly
            // 
            this.checkPSpeedFly.AutoSize = true;
            this.checkPSpeedFly.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkPSpeedFly.Location = new System.Drawing.Point(6, 101);
            this.checkPSpeedFly.Name = "checkPSpeedFly";
            this.checkPSpeedFly.Size = new System.Drawing.Size(145, 17);
            this.checkPSpeedFly.TabIndex = 14;
            this.checkPSpeedFly.Text = "Infinite P-speed and flight";
            this.checkPSpeedFly.UseVisualStyleBackColor = true;
            // 
            // checkScore
            // 
            this.checkScore.AutoSize = true;
            this.checkScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkScore.Location = new System.Drawing.Point(174, 23);
            this.checkScore.Name = "checkScore";
            this.checkScore.Size = new System.Drawing.Size(54, 17);
            this.checkScore.TabIndex = 15;
            this.checkScore.Text = "Score";
            this.checkScore.UseVisualStyleBackColor = true;
            // 
            // numericStars
            // 
            this.numericStars.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericStars.Location = new System.Drawing.Point(62, 46);
            this.numericStars.Name = "numericStars";
            this.numericStars.Size = new System.Drawing.Size(50, 20);
            this.numericStars.TabIndex = 19;
            // 
            // numericScore
            // 
            this.numericScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericScore.Location = new System.Drawing.Point(174, 46);
            this.numericScore.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.numericScore.Name = "numericScore";
            this.numericScore.Size = new System.Drawing.Size(80, 20);
            this.numericScore.TabIndex = 20;
            // 
            // numericTime
            // 
            this.numericTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericTime.Location = new System.Drawing.Point(118, 46);
            this.numericTime.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.numericTime.Name = "numericTime";
            this.numericTime.Size = new System.Drawing.Size(50, 20);
            this.numericTime.TabIndex = 21;
            this.numericTime.Value = new decimal(new int[] {
            150,
            0,
            0,
            0});
            // 
            // numericLives
            // 
            this.numericLives.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericLives.Location = new System.Drawing.Point(6, 46);
            this.numericLives.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericLives.Name = "numericLives";
            this.numericLives.Size = new System.Drawing.Size(50, 20);
            this.numericLives.TabIndex = 22;
            this.numericLives.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // checkRespawn
            // 
            this.checkRespawn.AutoSize = true;
            this.checkRespawn.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F);
            this.checkRespawn.Location = new System.Drawing.Point(6, 193);
            this.checkRespawn.Name = "checkRespawn";
            this.checkRespawn.Size = new System.Drawing.Size(225, 17);
            this.checkRespawn.TabIndex = 23;
            this.checkRespawn.Text = "Respawn at stored coordinates after death";
            this.checkRespawn.UseVisualStyleBackColor = true;
            // 
            // checkSPREADMYWINGS
            // 
            this.checkSPREADMYWINGS.AutoSize = true;
            this.checkSPREADMYWINGS.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkSPREADMYWINGS.Location = new System.Drawing.Point(6, 147);
            this.checkSPREADMYWINGS.Name = "checkSPREADMYWINGS";
            this.checkSPREADMYWINGS.Size = new System.Drawing.Size(129, 17);
            this.checkSPREADMYWINGS.TabIndex = 24;
            this.checkSPREADMYWINGS.Text = "SPREAD MY WINGS";
            this.checkSPREADMYWINGS.UseVisualStyleBackColor = true;
            // 
            // groupFreeze
            // 
            this.groupFreeze.Controls.Add(this.comboScoreRandomizer);
            this.groupFreeze.Controls.Add(this.checkScoreRandomizer);
            this.groupFreeze.Controls.Add(this.checkIframes);
            this.groupFreeze.Controls.Add(this.checkSPREADMYWINGS);
            this.groupFreeze.Controls.Add(this.buttonPowerup);
            this.groupFreeze.Controls.Add(this.checkRespawn);
            this.groupFreeze.Controls.Add(this.numericLives);
            this.groupFreeze.Controls.Add(this.numericTime);
            this.groupFreeze.Controls.Add(this.numericScore);
            this.groupFreeze.Controls.Add(this.numericStars);
            this.groupFreeze.Controls.Add(this.checkScore);
            this.groupFreeze.Controls.Add(this.checkPSpeedFly);
            this.groupFreeze.Controls.Add(this.checkPowerup);
            this.groupFreeze.Controls.Add(this.checkLives);
            this.groupFreeze.Controls.Add(this.checkTime);
            this.groupFreeze.Controls.Add(this.checkStars);
            this.groupFreeze.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupFreeze.Location = new System.Drawing.Point(225, 27);
            this.groupFreeze.Name = "groupFreeze";
            this.groupFreeze.Size = new System.Drawing.Size(260, 265);
            this.groupFreeze.TabIndex = 15;
            this.groupFreeze.TabStop = false;
            this.groupFreeze.Text = "Freeze/set values";
            // 
            // comboScoreRandomizer
            // 
            this.comboScoreRandomizer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboScoreRandomizer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboScoreRandomizer.FormattingEnabled = true;
            this.comboScoreRandomizer.Items.AddRange(new object[] {
            "Multiples of 10",
            "Very random"});
            this.comboScoreRandomizer.Location = new System.Drawing.Point(123, 168);
            this.comboScoreRandomizer.Name = "comboScoreRandomizer";
            this.comboScoreRandomizer.Size = new System.Drawing.Size(121, 21);
            this.comboScoreRandomizer.TabIndex = 27;
            // 
            // checkScoreRandomizer
            // 
            this.checkScoreRandomizer.AutoSize = true;
            this.checkScoreRandomizer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkScoreRandomizer.Location = new System.Drawing.Point(6, 170);
            this.checkScoreRandomizer.Name = "checkScoreRandomizer";
            this.checkScoreRandomizer.Size = new System.Drawing.Size(108, 17);
            this.checkScoreRandomizer.TabIndex = 26;
            this.checkScoreRandomizer.Text = "Score randomizer";
            this.checkScoreRandomizer.UseVisualStyleBackColor = true;
            // 
            // checkIframes
            // 
            this.checkIframes.AutoSize = true;
            this.checkIframes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkIframes.Location = new System.Drawing.Point(6, 124);
            this.checkIframes.Name = "checkIframes";
            this.checkIframes.Size = new System.Drawing.Size(111, 17);
            this.checkIframes.TabIndex = 25;
            this.checkIframes.Text = "Invincibility frames";
            this.checkIframes.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.helpAboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(493, 24);
            this.menuStrip1.TabIndex = 19;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.settingsToolStripMenuItem.Text = "Hotkeys";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // helpAboutToolStripMenuItem
            // 
            this.helpAboutToolStripMenuItem.Name = "helpAboutToolStripMenuItem";
            this.helpAboutToolStripMenuItem.Size = new System.Drawing.Size(82, 20);
            this.helpAboutToolStripMenuItem.Text = "Help/About";
            this.helpAboutToolStripMenuItem.Click += new System.EventHandler(this.helpAboutToolStripMenuItem_Click);
            // 
            // groupSaves
            // 
            this.groupSaves.Controls.Add(this.buttonDelete);
            this.groupSaves.Controls.Add(this.buttonNextScreen);
            this.groupSaves.Controls.Add(this.comboSaves);
            this.groupSaves.Controls.Add(this.buttonPreviousScreen);
            this.groupSaves.Controls.Add(this.buttonSave);
            this.groupSaves.Controls.Add(this.checkSave);
            this.groupSaves.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupSaves.Location = new System.Drawing.Point(12, 220);
            this.groupSaves.Name = "groupSaves";
            this.groupSaves.Size = new System.Drawing.Size(207, 124);
            this.groupSaves.TabIndex = 34;
            this.groupSaves.TabStop = false;
            this.groupSaves.Text = "Save states";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(229, 301);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 15);
            this.label6.TabIndex = 35;
            this.label6.Text = "Stage timer:";
            // 
            // labelTimerStage
            // 
            this.labelTimerStage.AutoSize = true;
            this.labelTimerStage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimerStage.Location = new System.Drawing.Point(331, 302);
            this.labelTimerStage.Name = "labelTimerStage";
            this.labelTimerStage.Size = new System.Drawing.Size(62, 15);
            this.labelTimerStage.TabIndex = 37;
            this.labelTimerStage.Text = "00:00.000";
            // 
            // labelTimerSaveState
            // 
            this.labelTimerSaveState.AutoSize = true;
            this.labelTimerSaveState.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTimerSaveState.Location = new System.Drawing.Point(331, 320);
            this.labelTimerSaveState.Name = "labelTimerSaveState";
            this.labelTimerSaveState.Size = new System.Drawing.Size(62, 15);
            this.labelTimerSaveState.TabIndex = 42;
            this.labelTimerSaveState.Text = "00:00.000";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(229, 320);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(97, 15);
            this.label10.TabIndex = 41;
            this.label10.Text = "Save state timer:";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(493, 448);
            this.Controls.Add(this.labelTimerSaveState);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.labelTimerStage);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupSaves);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupPspeed);
            this.Controls.Add(this.groupCoordinates);
            this.Controls.Add(this.groupFreeze);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.Text = "SuperMarisaWorldPrac v1.5.1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainWindow_FormClosed);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.groupCoordinates.ResumeLayout(false);
            this.groupCoordinates.PerformLayout();
            this.groupPspeed.ResumeLayout(false);
            this.groupPspeed.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericStars)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericScore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLives)).EndInit();
            this.groupFreeze.ResumeLayout(false);
            this.groupFreeze.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupSaves.ResumeLayout(false);
            this.groupSaves.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.Label labelY;
        private System.Windows.Forms.Label labelP;
        private System.Windows.Forms.ProgressBar pSpeedBar;
        private System.Windows.Forms.GroupBox groupCoordinates;
        private System.Windows.Forms.Button buttonGo;
        private System.Windows.Forms.TextBox textY;
        private System.Windows.Forms.TextBox textX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupPspeed;
        private System.Windows.Forms.Label labelJumpP;
        private System.Windows.Forms.Button buttonLoad;
        private System.Windows.Forms.Button buttonStore;
        private System.Windows.Forms.Button buttonPowerup;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.ComboBox comboSaves;
        private System.Windows.Forms.CheckBox checkSave;
        private System.Windows.Forms.Label labelStoredStars;
        private System.Windows.Forms.Label labelStoredScore;
        private System.Windows.Forms.Label labelStoredTime;
        private System.Windows.Forms.Label labelStoredY;
        private System.Windows.Forms.Label labelStoredX;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;
        private System.Windows.Forms.Button buttonNextScreen;
        private System.Windows.Forms.Button buttonPreviousScreen;
        private System.Windows.Forms.CheckBox checkStars;
        private System.Windows.Forms.CheckBox checkTime;
        private System.Windows.Forms.CheckBox checkLives;
        private System.Windows.Forms.CheckBox checkPowerup;
        private System.Windows.Forms.CheckBox checkPSpeedFly;
        private System.Windows.Forms.CheckBox checkScore;
        private System.Windows.Forms.NumericUpDown numericStars;
        private System.Windows.Forms.NumericUpDown numericScore;
        private System.Windows.Forms.NumericUpDown numericTime;
        private System.Windows.Forms.NumericUpDown numericLives;
        private System.Windows.Forms.CheckBox checkRespawn;
        private System.Windows.Forms.CheckBox checkSPREADMYWINGS;
        private System.Windows.Forms.GroupBox groupFreeze;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpAboutToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupSaves;
        private System.Windows.Forms.CheckBox checkIframes;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelTimerStage;
        private System.Windows.Forms.Label labelTimerSaveState;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkScoreRandomizer;
        private System.Windows.Forms.ComboBox comboScoreRandomizer;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ProgressBar flightBar;
    }
}

