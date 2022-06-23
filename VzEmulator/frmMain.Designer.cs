namespace VzEmulator
{
    partial class frmMain
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblFps = new System.Windows.Forms.Label();
            this.lblInstructionsPerSecond = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnGrMode = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnScale = new System.Windows.Forms.Button();
            this.btnSmooth = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMCEnd = new System.Windows.Forms.TextBox();
            this.txtMCStart = new System.Windows.Forms.TextBox();
            this.btnQuickLoad = new System.Windows.Forms.Button();
            this.btnQuickSave = new System.Windows.Forms.Button();
            this.btnColour = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emulationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.developmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDiskImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveDiskImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openvzFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBasicProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMachinecodeProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMemoryImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMemoryImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unPauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.execMachinecodeProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.integerScalingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleGraphicsModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRegistersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.traceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.Green;
            this.textBox1.Font = new System.Drawing.Font("Consolas", 10.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.ForeColor = System.Drawing.Color.Lime;
            this.textBox1.Location = new System.Drawing.Point(477, 91);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(921, 385);
            this.textBox1.TabIndex = 1;
            // 
            // lblFps
            // 
            this.lblFps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFps.AutoSize = true;
            this.lblFps.Location = new System.Drawing.Point(858, 7);
            this.lblFps.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFps.Name = "lblFps";
            this.lblFps.Size = new System.Drawing.Size(27, 13);
            this.lblFps.TabIndex = 2;
            this.lblFps.Text = "FPS";
            // 
            // lblInstructionsPerSecond
            // 
            this.lblInstructionsPerSecond.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInstructionsPerSecond.AutoSize = true;
            this.lblInstructionsPerSecond.Location = new System.Drawing.Point(858, 27);
            this.lblInstructionsPerSecond.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInstructionsPerSecond.Name = "lblInstructionsPerSecond";
            this.lblInstructionsPerSecond.Size = new System.Drawing.Size(24, 13);
            this.lblInstructionsPerSecond.TabIndex = 2;
            this.lblInstructionsPerSecond.Text = "IPS";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(826, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "FPS";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(826, 27);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "IPS";
            // 
            // btnGrMode
            // 
            this.btnGrMode.Location = new System.Drawing.Point(116, 37);
            this.btnGrMode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnGrMode.Name = "btnGrMode";
            this.btnGrMode.Size = new System.Drawing.Size(46, 24);
            this.btnGrMode.TabIndex = 3;
            this.btnGrMode.Text = "Mode";
            this.btnGrMode.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Location = new System.Drawing.Point(0, 93);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(902, 383);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.btnScale);
            this.pnlTop.Controls.Add(this.btnSmooth);
            this.pnlTop.Controls.Add(this.label3);
            this.pnlTop.Controls.Add(this.txtMCEnd);
            this.pnlTop.Controls.Add(this.txtMCStart);
            this.pnlTop.Controls.Add(this.btnQuickLoad);
            this.pnlTop.Controls.Add(this.btnQuickSave);
            this.pnlTop.Controls.Add(this.lblFps);
            this.pnlTop.Controls.Add(this.btnColour);
            this.pnlTop.Controls.Add(this.btnGrMode);
            this.pnlTop.Controls.Add(this.lblInstructionsPerSecond);
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Controls.Add(this.label2);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 24);
            this.pnlTop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(902, 69);
            this.pnlTop.TabIndex = 8;
            // 
            // btnScale
            // 
            this.btnScale.Location = new System.Drawing.Point(16, 37);
            this.btnScale.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnScale.Name = "btnScale";
            this.btnScale.Size = new System.Drawing.Size(48, 24);
            this.btnScale.TabIndex = 10;
            this.btnScale.Text = "Scale";
            this.btnScale.UseVisualStyleBackColor = true;
            // 
            // btnSmooth
            // 
            this.btnSmooth.Location = new System.Drawing.Point(66, 37);
            this.btnSmooth.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSmooth.Name = "btnSmooth";
            this.btnSmooth.Size = new System.Drawing.Size(48, 24);
            this.btnSmooth.TabIndex = 10;
            this.btnSmooth.Text = "Smooth";
            this.btnSmooth.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(556, 44);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "-";
            // 
            // txtMCEnd
            // 
            this.txtMCEnd.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMCEnd.Location = new System.Drawing.Point(568, 41);
            this.txtMCEnd.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtMCEnd.Name = "txtMCEnd";
            this.txtMCEnd.Size = new System.Drawing.Size(70, 23);
            this.txtMCEnd.TabIndex = 8;
            this.txtMCEnd.Leave += new System.EventHandler(this.txtMCEnd_Leave);
            this.txtMCEnd.Validating += new System.ComponentModel.CancelEventHandler(this.txtMCEnd_Validating);
            // 
            // txtMCStart
            // 
            this.txtMCStart.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMCStart.Location = new System.Drawing.Point(486, 41);
            this.txtMCStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtMCStart.Name = "txtMCStart";
            this.txtMCStart.Size = new System.Drawing.Size(70, 23);
            this.txtMCStart.TabIndex = 8;
            this.txtMCStart.Leave += new System.EventHandler(this.txtMCStart_Leave);
            this.txtMCStart.Validating += new System.ComponentModel.CancelEventHandler(this.txtMCStart_Validating);
            // 
            // btnQuickLoad
            // 
            this.btnQuickLoad.Location = new System.Drawing.Point(652, 37);
            this.btnQuickLoad.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnQuickLoad.Name = "btnQuickLoad";
            this.btnQuickLoad.Size = new System.Drawing.Size(66, 24);
            this.btnQuickLoad.TabIndex = 5;
            this.btnQuickLoad.Text = "Quickload";
            this.btnQuickLoad.UseVisualStyleBackColor = true;
            this.btnQuickLoad.Click += new System.EventHandler(this.btnQuickLoad_Click);
            // 
            // btnQuickSave
            // 
            this.btnQuickSave.Location = new System.Drawing.Point(652, 10);
            this.btnQuickSave.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnQuickSave.Name = "btnQuickSave";
            this.btnQuickSave.Size = new System.Drawing.Size(66, 24);
            this.btnQuickSave.TabIndex = 5;
            this.btnQuickSave.Text = "Quicksave";
            this.btnQuickSave.UseVisualStyleBackColor = true;
            this.btnQuickSave.Click += new System.EventHandler(this.btnQuickSave_Click);
            // 
            // btnColour
            // 
            this.btnColour.Location = new System.Drawing.Point(166, 37);
            this.btnColour.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnColour.Name = "btnColour";
            this.btnColour.Size = new System.Drawing.Size(46, 24);
            this.btnColour.TabIndex = 3;
            this.btnColour.Text = "Colour";
            this.btnColour.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.emulationToolStripMenuItem,
            this.displayToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.developmentToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(902, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openDiskImageToolStripMenuItem,
            this.saveDiskImageToolStripMenuItem,
            this.openvzFileToolStripMenuItem,
            this.saveBasicProgramToolStripMenuItem,
            this.saveMachinecodeProgramToolStripMenuItem,
            this.openMemoryImageToolStripMenuItem,
            this.saveMemoryImageToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // emulationToolStripMenuItem
            // 
            this.emulationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.pauseToolStripMenuItem,
            this.unPauseToolStripMenuItem,
            this.execMachinecodeProgramToolStripMenuItem});
            this.emulationToolStripMenuItem.Name = "emulationToolStripMenuItem";
            this.emulationToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.emulationToolStripMenuItem.Text = "Emulation";
            // 
            // displayToolStripMenuItem
            // 
            this.displayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.integerScalingToolStripMenuItem,
            this.smoothingToolStripMenuItem,
            this.toggleGraphicsModeToolStripMenuItem,
            this.colourToolStripMenuItem});
            this.displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            this.displayToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.displayToolStripMenuItem.Text = "Display";
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showRegistersToolStripMenuItem,
            this.showMemoryToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // developmentToolStripMenuItem
            // 
            this.developmentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showFontToolStripMenuItem,
            this.traceToolStripMenuItem});
            this.developmentToolStripMenuItem.Name = "developmentToolStripMenuItem";
            this.developmentToolStripMenuItem.Size = new System.Drawing.Size(90, 20);
            this.developmentToolStripMenuItem.Text = "Development";
            // 
            // openDiskImageToolStripMenuItem
            // 
            this.openDiskImageToolStripMenuItem.Name = "openDiskImageToolStripMenuItem";
            this.openDiskImageToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openDiskImageToolStripMenuItem.Text = "Open Disk Image";
            this.openDiskImageToolStripMenuItem.Click += new System.EventHandler(this.openDiskImageToolStripMenuItem_Click);
            // 
            // saveDiskImageToolStripMenuItem
            // 
            this.saveDiskImageToolStripMenuItem.Name = "saveDiskImageToolStripMenuItem";
            this.saveDiskImageToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.saveDiskImageToolStripMenuItem.Text = "Save Disk Image";
            this.saveDiskImageToolStripMenuItem.Click += new System.EventHandler(this.saveDiskImageToolStripMenuItem_Click);
            // 
            // openvzFileToolStripMenuItem
            // 
            this.openvzFileToolStripMenuItem.Name = "openvzFileToolStripMenuItem";
            this.openvzFileToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openvzFileToolStripMenuItem.Text = "Open .vz File";
            this.openvzFileToolStripMenuItem.Click += new System.EventHandler(this.openvzFileToolStripMenuItem_Click);
            // 
            // saveBasicProgramToolStripMenuItem
            // 
            this.saveBasicProgramToolStripMenuItem.Name = "saveBasicProgramToolStripMenuItem";
            this.saveBasicProgramToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.saveBasicProgramToolStripMenuItem.Text = "Save Basic Program";
            this.saveBasicProgramToolStripMenuItem.Click += new System.EventHandler(this.saveBasicProgramToolStripMenuItem_Click);
            // 
            // saveMachinecodeProgramToolStripMenuItem
            // 
            this.saveMachinecodeProgramToolStripMenuItem.Name = "saveMachinecodeProgramToolStripMenuItem";
            this.saveMachinecodeProgramToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.saveMachinecodeProgramToolStripMenuItem.Text = "Save Machine-code program";
            this.saveMachinecodeProgramToolStripMenuItem.Click += new System.EventHandler(this.saveMachinecodeProgramToolStripMenuItem_Click);
            // 
            // openMemoryImageToolStripMenuItem
            // 
            this.openMemoryImageToolStripMenuItem.Name = "openMemoryImageToolStripMenuItem";
            this.openMemoryImageToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openMemoryImageToolStripMenuItem.Text = "Open Memory Image";
            this.openMemoryImageToolStripMenuItem.Click += new System.EventHandler(this.openMemoryImageToolStripMenuItem_Click);
            // 
            // saveMemoryImageToolStripMenuItem
            // 
            this.saveMemoryImageToolStripMenuItem.Name = "saveMemoryImageToolStripMenuItem";
            this.saveMemoryImageToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.saveMemoryImageToolStripMenuItem.Text = "Save Memory Image";
            this.saveMemoryImageToolStripMenuItem.Click += new System.EventHandler(this.saveMemoryImageToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.startToolStripMenuItem.Text = "Start";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // pauseToolStripMenuItem
            // 
            this.pauseToolStripMenuItem.Enabled = false;
            this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
            this.pauseToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.pauseToolStripMenuItem.Text = "Pause";
            this.pauseToolStripMenuItem.Click += new System.EventHandler(this.pauseToolStripMenuItem_Click);
            // 
            // unPauseToolStripMenuItem
            // 
            this.unPauseToolStripMenuItem.Enabled = false;
            this.unPauseToolStripMenuItem.Name = "unPauseToolStripMenuItem";
            this.unPauseToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.unPauseToolStripMenuItem.Text = "Un-Pause";
            this.unPauseToolStripMenuItem.Click += new System.EventHandler(this.unPauseToolStripMenuItem_Click);
            // 
            // execMachinecodeProgramToolStripMenuItem
            // 
            this.execMachinecodeProgramToolStripMenuItem.Name = "execMachinecodeProgramToolStripMenuItem";
            this.execMachinecodeProgramToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.execMachinecodeProgramToolStripMenuItem.Text = "Exec machine-code program";
            this.execMachinecodeProgramToolStripMenuItem.Click += new System.EventHandler(this.execMachinecodeProgramToolStripMenuItem_Click);
            // 
            // integerScalingToolStripMenuItem
            // 
            this.integerScalingToolStripMenuItem.CheckOnClick = true;
            this.integerScalingToolStripMenuItem.Name = "integerScalingToolStripMenuItem";
            this.integerScalingToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.integerScalingToolStripMenuItem.Text = "Integer Scaling";
            this.integerScalingToolStripMenuItem.Click += new System.EventHandler(this.integerScalingToolStripMenuItem_Click);
            // 
            // smoothingToolStripMenuItem
            // 
            this.smoothingToolStripMenuItem.CheckOnClick = true;
            this.smoothingToolStripMenuItem.Name = "smoothingToolStripMenuItem";
            this.smoothingToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.smoothingToolStripMenuItem.Text = "Smoothing";
            this.smoothingToolStripMenuItem.Click += new System.EventHandler(this.smoothingToolStripMenuItem_Click);
            // 
            // toggleGraphicsModeToolStripMenuItem
            // 
            this.toggleGraphicsModeToolStripMenuItem.CheckOnClick = true;
            this.toggleGraphicsModeToolStripMenuItem.Name = "toggleGraphicsModeToolStripMenuItem";
            this.toggleGraphicsModeToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.toggleGraphicsModeToolStripMenuItem.Text = "Toggle graphics mode";
            this.toggleGraphicsModeToolStripMenuItem.Click += new System.EventHandler(this.toggleGraphicsModeToolStripMenuItem_Click);
            // 
            // colourToolStripMenuItem
            // 
            this.colourToolStripMenuItem.CheckOnClick = true;
            this.colourToolStripMenuItem.Name = "colourToolStripMenuItem";
            this.colourToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.colourToolStripMenuItem.Text = "Colour";
            this.colourToolStripMenuItem.Click += new System.EventHandler(this.colourToolStripMenuItem_Click);
            // 
            // showRegistersToolStripMenuItem
            // 
            this.showRegistersToolStripMenuItem.Name = "showRegistersToolStripMenuItem";
            this.showRegistersToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showRegistersToolStripMenuItem.Text = "Show Registers";
            this.showRegistersToolStripMenuItem.Click += new System.EventHandler(this.showRegistersToolStripMenuItem_Click);
            // 
            // showMemoryToolStripMenuItem
            // 
            this.showMemoryToolStripMenuItem.Name = "showMemoryToolStripMenuItem";
            this.showMemoryToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showMemoryToolStripMenuItem.Text = "Show Memory";
            // 
            // showFontToolStripMenuItem
            // 
            this.showFontToolStripMenuItem.Name = "showFontToolStripMenuItem";
            this.showFontToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showFontToolStripMenuItem.Text = "Show Font";
            // 
            // traceToolStripMenuItem
            // 
            this.traceToolStripMenuItem.CheckOnClick = true;
            this.traceToolStripMenuItem.Name = "traceToolStripMenuItem";
            this.traceToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.traceToolStripMenuItem.Text = "Trace";
            this.traceToolStripMenuItem.Click += new System.EventHandler(this.traceToolStripMenuItem_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(902, 451);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "frmMain";
            this.Text = "VZ300 Emulator";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblFps;
        private System.Windows.Forms.Label lblInstructionsPerSecond;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnGrMode;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMCEnd;
        private System.Windows.Forms.TextBox txtMCStart;
        private System.Windows.Forms.Button btnQuickSave;
        private System.Windows.Forms.Button btnQuickLoad;
        private System.Windows.Forms.Button btnScale;
        private System.Windows.Forms.Button btnSmooth;
        private System.Windows.Forms.Button btnColour;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emulationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDiskImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveDiskImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openvzFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveBasicProgramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMachinecodeProgramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMemoryImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMemoryImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unPauseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem execMachinecodeProgramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem integerScalingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleGraphicsModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colourToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem developmentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showRegistersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem traceToolStripMenuItem;
    }
}

