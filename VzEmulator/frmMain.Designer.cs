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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtMCEnd = new System.Windows.Forms.TextBox();
            this.txtMCStart = new System.Windows.Forms.TextBox();
            this.btnQuickLoad = new System.Windows.Forms.Button();
            this.btnQuickSave = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDiskImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveDiskImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openvzFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveBasicProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMachinecodeProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMemoryImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMemoryImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emulationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unPauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.execMachinecodeProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.integerScalingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleGraphicsModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colourToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundEnabledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRegistersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.developmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.traceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.diskStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.numberConversionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.basicProgramListingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showStatsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.overlayPanel1 = new VzEmulatorControls.OverlayPanel();
            this.printerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 24);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(800, 608);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.StatusLabel);
            this.pnlTop.Controls.Add(this.label5);
            this.pnlTop.Controls.Add(this.label4);
            this.pnlTop.Controls.Add(this.txtMCEnd);
            this.pnlTop.Controls.Add(this.txtMCStart);
            this.pnlTop.Controls.Add(this.btnQuickLoad);
            this.pnlTop.Controls.Add(this.btnQuickSave);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlTop.Location = new System.Drawing.Point(0, 632);
            this.pnlTop.Margin = new System.Windows.Forms.Padding(2);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(800, 60);
            this.pnlTop.TabIndex = 8;
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(4, 6);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(0, 13);
            this.StatusLabel.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(388, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "End";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(166, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(141, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Machine-code program Start";
            // 
            // txtMCEnd
            // 
            this.txtMCEnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtMCEnd.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMCEnd.Location = new System.Drawing.Point(419, 34);
            this.txtMCEnd.Margin = new System.Windows.Forms.Padding(2);
            this.txtMCEnd.Name = "txtMCEnd";
            this.txtMCEnd.Size = new System.Drawing.Size(70, 23);
            this.txtMCEnd.TabIndex = 8;
            this.txtMCEnd.Leave += new System.EventHandler(this.txtMCEnd_Leave);
            this.txtMCEnd.Validating += new System.ComponentModel.CancelEventHandler(this.txtMCEnd_Validating);
            // 
            // txtMCStart
            // 
            this.txtMCStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtMCStart.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMCStart.Location = new System.Drawing.Point(312, 34);
            this.txtMCStart.Margin = new System.Windows.Forms.Padding(2);
            this.txtMCStart.Name = "txtMCStart";
            this.txtMCStart.Size = new System.Drawing.Size(70, 23);
            this.txtMCStart.TabIndex = 8;
            this.txtMCStart.Leave += new System.EventHandler(this.txtMCStart_Leave);
            this.txtMCStart.Validating += new System.ComponentModel.CancelEventHandler(this.txtMCStart_Validating);
            // 
            // btnQuickLoad
            // 
            this.btnQuickLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnQuickLoad.Location = new System.Drawing.Point(72, 34);
            this.btnQuickLoad.Margin = new System.Windows.Forms.Padding(2);
            this.btnQuickLoad.Name = "btnQuickLoad";
            this.btnQuickLoad.Size = new System.Drawing.Size(66, 24);
            this.btnQuickLoad.TabIndex = 5;
            this.btnQuickLoad.Text = "Quickload";
            this.btnQuickLoad.UseVisualStyleBackColor = true;
            this.btnQuickLoad.Click += new System.EventHandler(this.btnQuickLoad_Click);
            // 
            // btnQuickSave
            // 
            this.btnQuickSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnQuickSave.Location = new System.Drawing.Point(2, 34);
            this.btnQuickSave.Margin = new System.Windows.Forms.Padding(2);
            this.btnQuickSave.Name = "btnQuickSave";
            this.btnQuickSave.Size = new System.Drawing.Size(66, 24);
            this.btnQuickSave.TabIndex = 5;
            this.btnQuickSave.Text = "Quicksave";
            this.btnQuickSave.UseVisualStyleBackColor = true;
            this.btnQuickSave.Click += new System.EventHandler(this.btnQuickSave_Click);
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
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
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
            // openDiskImageToolStripMenuItem
            // 
            this.openDiskImageToolStripMenuItem.Name = "openDiskImageToolStripMenuItem";
            this.openDiskImageToolStripMenuItem.Size = new System.Drawing.Size(227, 22);
            this.openDiskImageToolStripMenuItem.Text = "Open Disk Image";
            this.openDiskImageToolStripMenuItem.Click += new System.EventHandler(this.openDiskImageToolStripMenuItem_Click);
            // 
            // saveDiskImageToolStripMenuItem
            // 
            this.saveDiskImageToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
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
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
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
            // displayToolStripMenuItem
            // 
            this.displayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.integerScalingToolStripMenuItem,
            this.smoothingToolStripMenuItem,
            this.toggleGraphicsModeToolStripMenuItem,
            this.colourToolStripMenuItem,
            this.soundEnabledToolStripMenuItem,
            this.printerToolStripMenuItem});
            this.displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            this.displayToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.displayToolStripMenuItem.Text = "Display";
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
            // soundEnabledToolStripMenuItem
            // 
            this.soundEnabledToolStripMenuItem.CheckOnClick = true;
            this.soundEnabledToolStripMenuItem.Name = "soundEnabledToolStripMenuItem";
            this.soundEnabledToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.soundEnabledToolStripMenuItem.Text = "Sound (Experimental)";
            this.soundEnabledToolStripMenuItem.Click += new System.EventHandler(this.soundEnabledToolStripMenuItem_Click);
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
            this.showMemoryToolStripMenuItem.Click += new System.EventHandler(this.showMemoryToolStripMenuItem_Click);
            // 
            // developmentToolStripMenuItem
            // 
            this.developmentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showFontToolStripMenuItem,
            this.traceToolStripMenuItem,
            this.diskStatusToolStripMenuItem,
            this.numberConversionToolStripMenuItem,
            this.basicProgramListingToolStripMenuItem,
            this.showStatsToolStripMenuItem});
            this.developmentToolStripMenuItem.Name = "developmentToolStripMenuItem";
            this.developmentToolStripMenuItem.Size = new System.Drawing.Size(90, 20);
            this.developmentToolStripMenuItem.Text = "Development";
            // 
            // showFontToolStripMenuItem
            // 
            this.showFontToolStripMenuItem.Name = "showFontToolStripMenuItem";
            this.showFontToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.showFontToolStripMenuItem.Text = "Show Font";
            this.showFontToolStripMenuItem.Click += new System.EventHandler(this.showFontToolStripMenuItem_Click);
            // 
            // traceToolStripMenuItem
            // 
            this.traceToolStripMenuItem.CheckOnClick = true;
            this.traceToolStripMenuItem.Name = "traceToolStripMenuItem";
            this.traceToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.traceToolStripMenuItem.Text = "Trace";
            this.traceToolStripMenuItem.Click += new System.EventHandler(this.traceToolStripMenuItem_Click);
            // 
            // diskStatusToolStripMenuItem
            // 
            this.diskStatusToolStripMenuItem.Name = "diskStatusToolStripMenuItem";
            this.diskStatusToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.diskStatusToolStripMenuItem.Text = "Disk Status";
            this.diskStatusToolStripMenuItem.Click += new System.EventHandler(this.diskStatusToolStripMenuItem_Click);
            // 
            // numberConversionToolStripMenuItem
            // 
            this.numberConversionToolStripMenuItem.Name = "numberConversionToolStripMenuItem";
            this.numberConversionToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.numberConversionToolStripMenuItem.Text = "Number Conversion";
            // 
            // basicProgramListingToolStripMenuItem
            // 
            this.basicProgramListingToolStripMenuItem.Name = "basicProgramListingToolStripMenuItem";
            this.basicProgramListingToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.basicProgramListingToolStripMenuItem.Text = "Basic Program Listing";
            // 
            // showStatsToolStripMenuItem
            // 
            this.showStatsToolStripMenuItem.CheckOnClick = true;
            this.showStatsToolStripMenuItem.Name = "showStatsToolStripMenuItem";
            this.showStatsToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.showStatsToolStripMenuItem.Text = "Show Stats";
            this.showStatsToolStripMenuItem.Click += new System.EventHandler(this.showStatsToolStripMenuItem_Click);
            // 
            // overlayPanel1
            // 
            this.overlayPanel1.Location = new System.Drawing.Point(561, 42);
            this.overlayPanel1.Name = "overlayPanel1";
            this.overlayPanel1.Size = new System.Drawing.Size(200, 100);
            this.overlayPanel1.TabIndex = 10;
            // 
            // printerToolStripMenuItem
            // 
            this.printerToolStripMenuItem.Name = "printerToolStripMenuItem";
            this.printerToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.printerToolStripMenuItem.Text = "Printer";
            this.printerToolStripMenuItem.Click += new System.EventHandler(this.printerToolStripMenuItem_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 692);
            this.Controls.Add(this.overlayPanel1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmMain";
            this.Text = "Paul\'s VZ-300 Emulator";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyUp);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.TextBox txtMCEnd;
        private System.Windows.Forms.TextBox txtMCStart;
        private System.Windows.Forms.Button btnQuickSave;
        private System.Windows.Forms.Button btnQuickLoad;
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
        private System.Windows.Forms.ToolStripMenuItem soundEnabledToolStripMenuItem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStripMenuItem diskStatusToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem numberConversionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem basicProgramListingToolStripMenuItem;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.ToolStripMenuItem showStatsToolStripMenuItem;
        private VzEmulatorControls.OverlayPanel overlayPanel1;
        private System.Windows.Forms.ToolStripMenuItem printerToolStripMenuItem;
    }
}

