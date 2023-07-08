using System;
using System.Windows.Forms;
using VzEmulator.Debugger;

namespace VzEmulator
{
    public partial class frmMain : Form, IMachineView
    {

        public MachinePresenter Presenter;
        private bool _isTrace;
        private const string QuickSaveFilename = "QuickSave1.img";
        private readonly int DefaultWidth = 816;
        private readonly int DefaultHeight = 731;

        Control IMachineView.RenderControl { get => pictureBox1; }

        void IMachineView.UpdateMCStart(ushort value)
        {
            if (!txtMCStart.Focused)
            {
                txtMCStart.Text = $"0x{value:X}";
            }
        }

        void IMachineView.UpdateMcEnd(ushort value)
        {
            if (!txtMCEnd.Focused)
            {
                txtMCEnd.Text = string.Format("0x{0:X}", value);
            }
        }

        void IMachineView.UpdateStats(MachineStats stats)
        {
            if (showStatsToolStripMenuItem.Checked)
            {
                overlayPanel1.TextElements.Add(("IPS", stats.InstructionsPerSecond.ToString()));
                //format CLK as decimal with 2 decimal places

                string actualClockSpeedMhz = $"{(stats.ClockCyclesPerSecond / 1_000_000m):0.00}Mhz";
                overlayPanel1.TextElements.Add(("CLK", actualClockSpeedMhz));
                overlayPanel1.TextElements.Add(("FPS", stats.FramesPerSecond.ToString()));
                if (overlayPanel1.TextElements.Count > 3)
                {
                    overlayPanel1.TextElements.RemoveRange(0, 3);
                }
            }
        }
        void IMachineView.RenderComplete()
        {
            //The main display has been rendered/painted so paint the overlay too
            if (showStatsToolStripMenuItem.Checked)
                overlayPanel1.Invalidate();
        }
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //todo remember previous
            Width = DefaultWidth;
            Height = DefaultHeight;

            foreach (Control ctrl in pnlTop.Controls)
            {
                ctrl.PreviewKeyDown += Button_PreviewKeyDown;
            }

            StartEmulation();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            Presenter.KeyDown(e.KeyValue, e.KeyData);
        }
        private void frmMain_KeyUp(object sender, KeyEventArgs e)
        {
            Presenter.KeyUp(e.KeyCode ^ e.KeyData);
            e.Handled = true;
        }

        private void Button_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void txtMCStart_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = ValidateDebugTextbox(sender as Control);
        }

        private void txtMCEnd_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = ValidateDebugTextbox(sender as Control);
        }

        private bool ValidateDebugTextbox(Control ctrl)
        {
            if (ctrl != null)
            {
                try
                {
                    MemUtils.StringToUShort(ctrl.Text);
                }
                catch (Exception)
                {
                    return true;
                }
            }
            return false;
        }

        private void txtMCStart_Leave(object sender, EventArgs e)
        {
            var value = MemUtils.StringToUShort(txtMCStart.Text);
            Presenter.StartMcProgram = value;
        }

        private void txtMCEnd_Leave(object sender, EventArgs e)
        {
            Presenter.EndMCProgram = MemUtils.StringToUShort(txtMCEnd.Text);
        }
        
        private void btnQuickSave_Click(object sender, EventArgs e)
        {
            Presenter.SaveImage(QuickSaveFilename);
        }

        private void btnQuickLoad_Click(object sender, EventArgs e)
        {
            try
            {
                Presenter.LoadImage(QuickSaveFilename);
            } catch (Exception ex)
            {
                Toast(ex.Message);
            }
         }

        private void Toast(string message)
        {
            StatusLabel.Text = message;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartEmulation();
        }

        private void StartEmulation()
        {
            Presenter.SoundEnabled = soundEnabledToolStripMenuItem.Checked;

            Presenter.Start();
            
            startToolStripMenuItem.Text = "Reset";
            pauseToolStripMenuItem.Enabled = true;
            unPauseToolStripMenuItem.Enabled = true;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.Pause();
        }

        private void unPauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.UnPause();
        }

        private void execMachinecodeProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.JumpToUsrExec();
        }

        private void openDiskImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Presenter.LoadDiskImage(openFileDialog.FileName);
            }
        }

        private void saveDiskImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Presenter.SaveDiskImage(saveFileDialog.FileName);
            }
        }

        private void openMemoryImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Presenter.LoadImage(dlg.FileName);
            }
        }

        private void saveMemoryImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Presenter.SaveImage(dlg.FileName);
            }
        }

        private void openvzFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var addressRange = Presenter.LoadFile(dlg.FileName);
                ((IMachineView)this).UpdateMCStart(addressRange.Start);
                ((IMachineView)this).UpdateMcEnd(addressRange.End);

            }
        }

        private void saveBasicProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Presenter.SaveBasicFile(dlg.FileName);
            }
        }

        private void saveMachinecodeProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Presenter.SaveMcFile(dlg.FileName);
            }
        }

        private void traceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _isTrace = !_isTrace;
            Presenter.EnableTrace(_isTrace);
        }

        private void showRegistersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.DebugRegisters();
        }
        private void showMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.DebugMemory();
        }

        private void integerScalingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.ToggleUseFixedScaling();
        }

        private void smoothingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.ToggleDisplaySmothing();
        }

        private void toggleGraphicsModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.ToggleGrahpicsMode();
        }

        private void colourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.ToggleColour();
        }

        private void showFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.DevelopmentShowFont();
        }

        private void soundEnabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SoundEnabled = soundEnabledToolStripMenuItem.Checked;
        }
        private void cassetteSoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.CassetteSoundStreamEnabled = cassetteSoundToolStripMenuItem.Checked;

        }
        private void soundTestToneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SoundTestTone = soundTestToneToolStripMenuItem.Checked;
        }
        private void frmMain_Resize(object sender, EventArgs e)
        {
            StatusLabel.Text = $"Width: {Width}, Height: {Height}";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void showStatsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            overlayPanel1.Visible = showStatsToolStripMenuItem.Checked;
        }

        private void diskStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmDriveWatcher();
            Presenter.AttachDriveWatcher(frm);
            frm.Show();
        }

        private void printerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmPrinterView();
            frm.PrinterOutput = Presenter.GetPrinterOutput();
            frm.Show();
        }

        private void developmentToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void recordCassetteToWavFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Prompt user for save file name and location
            var dlg = new SaveFileDialog() { AddExtension = true, DefaultExt = "wav", Filter = "Wav files (*.wav)|*.wav" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Presenter.StartRecordingCassette(dlg.FileName);
                //Show file recording widget
                //cassetteRecorder1.Visible = true;

            }
        }

        private void playWavFileInToCassetteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //prompt user for open file name and location 
            var dlg = new OpenFileDialog() { AddExtension = true, DefaultExt = "wav", Filter = "Wav files (*.wav)|*.wav" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                 Presenter.StartPlayingCassette(dlg.FileName);
                //Show file recording widget
                //cassetteRecorder1.Visible = true;

            }
        }

        private void audioToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dosRomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SetDosRomPresent(dosRomToolStripMenuItem.Checked);
            //todo general function to enable/disable peripherals
        }

        private void pOKE307770ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SetMemory(30777, new Byte[] { 0 });
        }

        private void toggleClockSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SetClockSync(toggleClockSyncToolStripMenuItem.Checked);
        }
    }
}
 