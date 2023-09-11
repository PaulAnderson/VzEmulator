using System;
using System.Drawing;
using System.IO;
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

            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = "";

            var statusUpdater = new StatusUpdater(toolStripStatusLabel1);
            Presenter.AttachDriveWatcher(statusUpdater);

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
            toolStripStatusLabel1.Text = message;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartEmulation();
        }

        private void StartEmulation()
        {
            Presenter.SoundEnabled = soundEnabledToolStripMenuItem.Checked;

            Presenter.Start();

            if (startToolStripMenuItem.Text == "Reset")
            {
                toolStripStatusLabel1.Text = "Emulation Reset";
            } else
            {
                toolStripStatusLabel1.Text = "Emulation started";
            }

            startToolStripMenuItem.Text = "Reset";
            pauseToolStripMenuItem.Enabled = true;
            unPauseToolStripMenuItem.Enabled = true;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.Pause();
            toolStripStatusLabel1.Text = "Emulation paused";
        }

        private void unPauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.UnPause();
            toolStripStatusLabel1.Text = "Emulation resumed";
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
                var fileName = Path.GetFileName(openFileDialog.FileName);
                toolStripStatusLabel1.Text = $"Opened Disk image {fileName} for in-memory editing. It must be saved to persist changes.";
            }
        }

        private void saveDiskImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Presenter.SaveDiskImage(saveFileDialog.FileName);
                var fileName = Path.GetFileName(saveFileDialog.FileName);
                toolStripStatusLabel1.Text = $"Saved Disk image {fileName}";
            }
        }

        private void reformatDiskSectorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //convert disk to standard track length, write sector data with regenerated headers
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Presenter.SaveDiskImage(saveFileDialog.FileName, reFormat: true);
                var fileName = Path.GetFileName(saveFileDialog.FileName);
                toolStripStatusLabel1.Text = $"Saved {fileName} in stardard sector format";
            }
        }

        private void openMemoryImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Presenter.LoadImage(dlg.FileName);
                var fileName = Path.GetFileName(dlg.FileName);
                toolStripStatusLabel1.Text = $"Opened memory image {fileName} ";
            }
        }

        private void saveMemoryImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Presenter.SaveImage(dlg.FileName);
                var fileName = Path.GetFileName(dlg.FileName);
                toolStripStatusLabel1.Text = $"Saved memory image {fileName} ";
            }
        }

        private void openvzFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var addressRange = Presenter.LoadFile(dlg.FileName);
                txtMCStart.Text = $"{addressRange.Start:X4}";
                txtMCEnd.Text = $"{addressRange.End:X4}";
                pictureBox1.Focus();

                //((IMachineView)this).UpdateMCStart(addressRange.Start);
                //((IMachineView)this).UpdateMcEnd(addressRange.End);

                var fileName = Path.GetFileName(dlg.FileName);
                toolStripStatusLabel1.Text = $"Opened program {fileName} ";

            }
        }

        private void saveBasicProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Presenter.SaveBasicFile(dlg.FileName);

                var fileName = Path.GetFileName(dlg.FileName);
                toolStripStatusLabel1.Text = $"Saved basic program {fileName} ";
            }
        }

        private void saveMachinecodeProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Presenter.SaveMcFile(dlg.FileName);

                var fileName = Path.GetFileName(dlg.FileName);
                toolStripStatusLabel1.Text = $"Saved MC program {fileName} ";
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
        private void editDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.EditDisk(1);
        }
        private void inDrive1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.EditDisk(1);
        }
        private void inDrive2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.EditDisk(2);
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
            Presenter.ToggleGraphicsMode();
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
            Toast($"Width: {Width}, Height: {Height}");
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

        private void mhzToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SetClockSpeed(0.1m);
        }

        private void Clock2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SetClockSpeed(1.0m);

        }

        private void Clock3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SetClockSpeed(VzConstants.ClockFrequencyMhz);

        }

        private void Clock4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SetClockSpeed(8.0m);

        }

        private void Clock5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.SetClockSpeed(16.0m);

        }

        private void Clock6ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        MachineRunner previewRunner;
        private void testPreviewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //todo new dialog to select file
            //User can browse to a folder, and preview all support files in that folder
            //Option to create thumbnails?
            //.vz files, .wav files?, disk images, rom files, memory images
            //todo virtual disk support - mount folder of .vz as disk image

            //Im thinking 2 panes - folder browse, and selected folder view, with a thumbnail and info for each file in a repeating tile
            //Filename, size, date, type (MC/Basic/Other), thumbnail, preview button, open button

            //Also - ability to save data files?  
            //Separate to this task - try bringing in the graphics chip from the ? (other emulator) and load the rom, see if it works
            //for disks, show the file list and perhaps the preview of each file on disk, or the first one
            //extract text and allow keyword search of all files seen

            var fbd = new FolderBrowserDialog() { SelectedPath = "C:\\git\\vz200\\" };
            var result = fbd.ShowDialog(this);
            if (result==DialogResult.OK)
            {
                var frmFolderView = new frmFolderView(this,Presenter, fbd.SelectedPath);
                frmFolderView.Show();
                //todo add a callback to load file
            }
            return;

            string dosRomFileName = "Roms/VZDOS.ROM";
            string romFilename = "Roms/VZ300.ROM";

            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            //todo load program, run for a set number of cycles, get image

            if (previewRunner == null)
            {
                  previewRunner = new MachineRunner(new Machine());

                //create runner for this and future previews
                previewRunner.Start(null);
                while (previewRunner.LatestImage == null)
                {
                    System.Threading.Thread.Sleep(0);
                }
            }
            var frm = new Form();
            previewRunner.LatestImage = null;
            previewRunner.LoadAndRunFile(openFileDialog.FileName);
            while (previewRunner.LatestImage == null)
            {
                System.Threading.Thread.Sleep(0);
            }
            frm.BackgroundImageLayout = ImageLayout.Stretch;
            frm.Size = new Size(256, 192);
            frm.BackgroundImage = previewRunner.LatestImage;
            frm.Show();
        }
     

        private void Run_Click(object sender, EventArgs e)
        {
            Presenter.TestExecRun();
        }

        private void pasteTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Presenter.PasteTextAsKeys(Clipboard.GetText());
        }

        private void copyTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Get text from screen
            var screenText = Presenter.GetScreenText();

            //Copy to clipboard
            Clipboard.SetText(screenText);
        }

        private void copyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var screen = Presenter.GetScreenImage();
            Clipboard.SetImage(screen);
        }

        private void captureScreenOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmScreenLogView();
            frm.ScreenOutput = Presenter.GetscreenOutputMonitor();
            frm.Show();
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void btnExecMachineCode_Click(object sender, EventArgs e)
        {
            Presenter.JumpToUsrExec();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Presenter.RunBasic();
        }
 
    }
}
 