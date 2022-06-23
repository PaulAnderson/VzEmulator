﻿using System;
using System.Windows.Forms;

namespace VzEmulator
{
    public partial class frmMain : Form, IMachineView
    {

        public MachinePresenter Presenter;
        private bool _isTrace;
        private const string QuickSaveFilename = "QuickSave1.img";

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
            lblInstructionsPerSecond.Text = stats.InstructionsPerSecond.ToString();
            lblFps.Text = stats.FramesPerSecond.ToString();
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in pnlTop.Controls)
            {
                ctrl.PreviewKeyDown += Button_PreviewKeyDown;
            }

            StartEmulation();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

        }



        //private string GetTextModeText()
        //{

        //    var sb = new StringBuilder();
        //    var offset = 0;
        //    for (var y = 0; y < 16; y++)
        //    {
        //        for (var x = 0; x < 32; x++)
        //        {
        //            //get value
        //            ushort pos;
        //            unchecked
        //            {
        //                pos = (ushort)(offset + y * 32 + x);
        //            }
        //            var value = videoMemory.Content[pos];

        //            //create string
        //            if (value < 0x20) value += 0x40;
        //            if (value >= 0x60 && value < 0x80) value -= 0x40;
        //            if (value > 32 && value <= 128)
        //            {
        //                sb.Append((char)value);
        //            } else if (value == 32 && videoMemory.Content[pos] == 32) //inverted space, cursor
        //            {
        //                sb.Append('#');
        //            }
        //            {
        //                sb.Append(' ');
        //            }
        //        }
        //        sb.AppendLine("");
        //    }

        //    return sb.ToString();
        //}

        private void btnPause_Click(object sender, EventArgs e)
        {
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
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

        private void btnTrace_Click(object sender, EventArgs e)
        {

        }

        private void btnExec_Click(object sender, EventArgs e)
        {
        }

        private void btnSaveBasic_Click(object sender, EventArgs e)
        {
        
        }
        private void btnSaveMC_Click(object sender, EventArgs e)
        {
         
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
      
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
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
        
        private void button3_Click(object sender, EventArgs e)
        {
        
        }

        private void btnQuickSave_Click(object sender, EventArgs e)
        {
            Presenter.SaveImage(QuickSaveFilename);
        }

        private void button4_Click(object sender, EventArgs e)
        {
          
        }
        private void btnQuickLoad_Click(object sender, EventArgs e)
        {
            Presenter.LoadImage(QuickSaveFilename);
        }

    
 


        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartEmulation();
        }

        private void StartEmulation()
        {
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
            Presenter.StartDebug();
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

        }

        private void btnGR_Click(object sender, EventArgs e)
        {
        }

        private void btnSmooth_Click(object sender, EventArgs e)
        {
            
        }

        private void btnScale_Click(object sender, EventArgs e)
        {
            
        }
        private void btnColour_Click(object sender, EventArgs e)
        {
 
        }
    }
}
