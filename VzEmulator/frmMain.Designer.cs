namespace VzEmulate2
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
            this.btnStart = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnContinue = new System.Windows.Forms.Button();
            this.lblFps = new System.Windows.Forms.Label();
            this.lblInstructionsPerSecond = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnTrace = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnExec = new System.Windows.Forms.Button();
            this.btnGrMode = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnSaveBasic = new System.Windows.Forms.Button();
            this.btnSaveMC = new System.Windows.Forms.Button();
            this.btnDebug = new System.Windows.Forms.Button();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnSaveDisk = new System.Windows.Forms.Button();
            this.btnScale = new System.Windows.Forms.Button();
            this.btnSmooth = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMCEnd = new System.Windows.Forms.TextBox();
            this.txtMCStart = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.btnQuickLoad = new System.Windows.Forms.Button();
            this.btnQuickSave = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.btnLoadDisk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(9, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(135, 37);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.Green;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("Consolas", 10.875F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.ForeColor = System.Drawing.Color.Lime;
            this.textBox1.Location = new System.Drawing.Point(0, 106);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(1353, 616);
            this.textBox1.TabIndex = 1;
            // 
            // btnPause
            // 
            this.btnPause.Enabled = false;
            this.btnPause.Location = new System.Drawing.Point(147, 12);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(135, 37);
            this.btnPause.TabIndex = 0;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnContinue
            // 
            this.btnContinue.Enabled = false;
            this.btnContinue.Location = new System.Drawing.Point(286, 12);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(135, 37);
            this.btnContinue.TabIndex = 0;
            this.btnContinue.Text = "Continue";
            this.btnContinue.UseVisualStyleBackColor = true;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // lblFps
            // 
            this.lblFps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFps.AutoSize = true;
            this.lblFps.Location = new System.Drawing.Point(1287, 11);
            this.lblFps.Name = "lblFps";
            this.lblFps.Size = new System.Drawing.Size(40, 20);
            this.lblFps.TabIndex = 2;
            this.lblFps.Text = "FPS";
            // 
            // lblInstructionsPerSecond
            // 
            this.lblInstructionsPerSecond.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInstructionsPerSecond.AutoSize = true;
            this.lblInstructionsPerSecond.Location = new System.Drawing.Point(1287, 42);
            this.lblInstructionsPerSecond.Name = "lblInstructionsPerSecond";
            this.lblInstructionsPerSecond.Size = new System.Drawing.Size(35, 20);
            this.lblInstructionsPerSecond.TabIndex = 2;
            this.lblInstructionsPerSecond.Text = "IPS";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1239, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "FPS";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1239, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "IPS";
            // 
            // btnTrace
            // 
            this.btnTrace.Location = new System.Drawing.Point(426, 12);
            this.btnTrace.Name = "btnTrace";
            this.btnTrace.Size = new System.Drawing.Size(75, 37);
            this.btnTrace.TabIndex = 0;
            this.btnTrace.Text = "Trace";
            this.btnTrace.UseVisualStyleBackColor = true;
            this.btnTrace.Click += new System.EventHandler(this.btnTrace_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(510, 15);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(99, 37);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnExec
            // 
            this.btnExec.Location = new System.Drawing.Point(510, 57);
            this.btnExec.Name = "btnExec";
            this.btnExec.Size = new System.Drawing.Size(99, 37);
            this.btnExec.TabIndex = 3;
            this.btnExec.Text = "Exec";
            this.btnExec.UseVisualStyleBackColor = true;
            this.btnExec.Click += new System.EventHandler(this.btnExec_Click);
            // 
            // btnGrMode
            // 
            this.btnGrMode.Location = new System.Drawing.Point(286, 57);
            this.btnGrMode.Name = "btnGrMode";
            this.btnGrMode.Size = new System.Drawing.Size(68, 37);
            this.btnGrMode.TabIndex = 3;
            this.btnGrMode.Text = "Mode";
            this.btnGrMode.UseVisualStyleBackColor = true;
            this.btnGrMode.Click += new System.EventHandler(this.btnGR_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Green;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 106);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(1353, 616);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // btnSaveBasic
            // 
            this.btnSaveBasic.Location = new System.Drawing.Point(615, 15);
            this.btnSaveBasic.Name = "btnSaveBasic";
            this.btnSaveBasic.Size = new System.Drawing.Size(99, 37);
            this.btnSaveBasic.TabIndex = 5;
            this.btnSaveBasic.Text = "Save Basic";
            this.btnSaveBasic.UseVisualStyleBackColor = true;
            this.btnSaveBasic.Click += new System.EventHandler(this.btnSaveBasic_Click);
            // 
            // btnSaveMC
            // 
            this.btnSaveMC.Location = new System.Drawing.Point(615, 57);
            this.btnSaveMC.Name = "btnSaveMC";
            this.btnSaveMC.Size = new System.Drawing.Size(99, 37);
            this.btnSaveMC.TabIndex = 6;
            this.btnSaveMC.Text = "Save MC";
            this.btnSaveMC.UseVisualStyleBackColor = true;
            this.btnSaveMC.Click += new System.EventHandler(this.btnSaveMC_Click);
            // 
            // btnDebug
            // 
            this.btnDebug.Location = new System.Drawing.Point(434, 62);
            this.btnDebug.Name = "btnDebug";
            this.btnDebug.Size = new System.Drawing.Size(66, 34);
            this.btnDebug.TabIndex = 7;
            this.btnDebug.Text = "Debug";
            this.btnDebug.UseVisualStyleBackColor = true;
            this.btnDebug.Click += new System.EventHandler(this.button2_Click);
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.btnLoadDisk);
            this.pnlTop.Controls.Add(this.btnSaveDisk);
            this.pnlTop.Controls.Add(this.btnScale);
            this.pnlTop.Controls.Add(this.btnSmooth);
            this.pnlTop.Controls.Add(this.label3);
            this.pnlTop.Controls.Add(this.txtMCEnd);
            this.pnlTop.Controls.Add(this.txtMCStart);
            this.pnlTop.Controls.Add(this.button4);
            this.pnlTop.Controls.Add(this.btnStart);
            this.pnlTop.Controls.Add(this.btnDebug);
            this.pnlTop.Controls.Add(this.btnPause);
            this.pnlTop.Controls.Add(this.btnSaveMC);
            this.pnlTop.Controls.Add(this.btnContinue);
            this.pnlTop.Controls.Add(this.btnQuickLoad);
            this.pnlTop.Controls.Add(this.btnQuickSave);
            this.pnlTop.Controls.Add(this.button3);
            this.pnlTop.Controls.Add(this.btnSaveBasic);
            this.pnlTop.Controls.Add(this.btnTrace);
            this.pnlTop.Controls.Add(this.lblFps);
            this.pnlTop.Controls.Add(this.btnGrMode);
            this.pnlTop.Controls.Add(this.lblInstructionsPerSecond);
            this.pnlTop.Controls.Add(this.btnExec);
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Controls.Add(this.btnLoad);
            this.pnlTop.Controls.Add(this.label2);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1353, 106);
            this.pnlTop.TabIndex = 8;
            // 
            // btnSaveDisk
            // 
            this.btnSaveDisk.Location = new System.Drawing.Point(1109, 12);
            this.btnSaveDisk.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSaveDisk.Name = "btnSaveDisk";
            this.btnSaveDisk.Size = new System.Drawing.Size(128, 37);
            this.btnSaveDisk.TabIndex = 11;
            this.btnSaveDisk.Text = "Save Disk";
            this.btnSaveDisk.UseVisualStyleBackColor = true;
            this.btnSaveDisk.Click += new System.EventHandler(this.btnSaveDisk_Click);
            // 
            // btnScale
            // 
            this.btnScale.Location = new System.Drawing.Point(134, 57);
            this.btnScale.Name = "btnScale";
            this.btnScale.Size = new System.Drawing.Size(72, 37);
            this.btnScale.TabIndex = 10;
            this.btnScale.Text = "Scale";
            this.btnScale.UseVisualStyleBackColor = true;
            this.btnScale.Click += new System.EventHandler(this.btnScale_Click);
            // 
            // btnSmooth
            // 
            this.btnSmooth.Location = new System.Drawing.Point(210, 57);
            this.btnSmooth.Name = "btnSmooth";
            this.btnSmooth.Size = new System.Drawing.Size(72, 37);
            this.btnSmooth.TabIndex = 10;
            this.btnSmooth.Text = "Smooth";
            this.btnSmooth.UseVisualStyleBackColor = true;
            this.btnSmooth.Click += new System.EventHandler(this.btnSmooth_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(834, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 20);
            this.label3.TabIndex = 9;
            this.label3.Text = "-";
            // 
            // txtMCEnd
            // 
            this.txtMCEnd.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMCEnd.Location = new System.Drawing.Point(852, 62);
            this.txtMCEnd.Name = "txtMCEnd";
            this.txtMCEnd.Size = new System.Drawing.Size(102, 31);
            this.txtMCEnd.TabIndex = 8;
            this.txtMCEnd.Leave += new System.EventHandler(this.txtMCEnd_Leave);
            this.txtMCEnd.Validating += new System.ComponentModel.CancelEventHandler(this.txtMCEnd_Validating);
            // 
            // txtMCStart
            // 
            this.txtMCStart.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMCStart.Location = new System.Drawing.Point(729, 62);
            this.txtMCStart.Name = "txtMCStart";
            this.txtMCStart.Size = new System.Drawing.Size(102, 31);
            this.txtMCStart.TabIndex = 8;
            this.txtMCStart.Leave += new System.EventHandler(this.txtMCStart_Leave);
            this.txtMCStart.Validating += new System.ComponentModel.CancelEventHandler(this.txtMCStart_Validating);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(837, 15);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(116, 37);
            this.button4.TabIndex = 0;
            this.button4.Text = "Load Image";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // btnQuickLoad
            // 
            this.btnQuickLoad.Location = new System.Drawing.Point(978, 57);
            this.btnQuickLoad.Name = "btnQuickLoad";
            this.btnQuickLoad.Size = new System.Drawing.Size(99, 37);
            this.btnQuickLoad.TabIndex = 5;
            this.btnQuickLoad.Text = "Quickload";
            this.btnQuickLoad.UseVisualStyleBackColor = true;
            this.btnQuickLoad.Click += new System.EventHandler(this.btnQuickLoad_Click);
            // 
            // btnQuickSave
            // 
            this.btnQuickSave.Location = new System.Drawing.Point(978, 15);
            this.btnQuickSave.Name = "btnQuickSave";
            this.btnQuickSave.Size = new System.Drawing.Size(99, 37);
            this.btnQuickSave.TabIndex = 5;
            this.btnQuickSave.Text = "Quicksave";
            this.btnQuickSave.UseVisualStyleBackColor = true;
            this.btnQuickSave.Click += new System.EventHandler(this.btnQuickSave_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(729, 15);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 37);
            this.button3.TabIndex = 5;
            this.button3.Text = "Save Image";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnLoadDisk
            // 
            this.btnLoadDisk.Location = new System.Drawing.Point(1109, 60);
            this.btnLoadDisk.Margin = new System.Windows.Forms.Padding(2);
            this.btnLoadDisk.Name = "btnLoadDisk";
            this.btnLoadDisk.Size = new System.Drawing.Size(128, 37);
            this.btnLoadDisk.TabIndex = 12;
            this.btnLoadDisk.Text = "Load Disk";
            this.btnLoadDisk.UseVisualStyleBackColor = true;
            this.btnLoadDisk.Click += new System.EventHandler(this.btnLoadDisk_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1353, 722);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.pnlTop);
            this.KeyPreview = true;
            this.Name = "frmMain";
            this.Text = "VZ300 Emulator";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.Label lblFps;
        private System.Windows.Forms.Label lblInstructionsPerSecond;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnTrace;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnExec;
        private System.Windows.Forms.Button btnGrMode;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnSaveBasic;
        private System.Windows.Forms.Button btnSaveMC;
        private System.Windows.Forms.Button btnDebug;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMCEnd;
        private System.Windows.Forms.TextBox txtMCStart;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnQuickSave;
        private System.Windows.Forms.Button btnQuickLoad;
        private System.Windows.Forms.Button btnScale;
        private System.Windows.Forms.Button btnSmooth;
        private System.Windows.Forms.Button btnSaveDisk;
        private System.Windows.Forms.Button btnLoadDisk;
    }
}

