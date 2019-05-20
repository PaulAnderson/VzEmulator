﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using VzEmulator.Debug;
using VzEmulator.Peripherals;
using VzEmulator.Screen;

namespace VzEmulator
{
    public partial class frmMain : Form
    {
        private ICpu cpu = new Z80dotNetCpuAdapter();
        bool cpuIsSetup = false;
        Timer interruptTimer;
        Timer watchTimer;
        Timer debugTimer;
        bool resetting;
        bool savingImage;
        bool loadingImage;
        string ImageFilename;
        string placeHolderRomFilename = "Roms/Placeholder.Rom";
        string romFilename = "Roms/ROM-FULL.DBG";
        const string quickSaveFilename = "QuickSave1.img";
        bool isTrace;
        long instructionsPerSecond;
        private MemUtils mem;
        IInterruptEnableFlag intSource;
        InstructionTracer tracer;
        ushort EndMCProgram;
        GraphicsPainter graphicsPainter;
        PeripheralRouter router = new PeripheralRouter();
        Drive drive = new Drive();
        Keyboard keyboard;
        OutputLatch outputLatch = new OutputLatch();
        Rom rom = new Rom();
        private VideoMemory videoMemory;
        IMemoryAccessor memory;
        FileIO fileIo;
        FileHandler fileHandler;

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

            SetupDevices();
        }

        private void SetupDevices()
        {
            intSource = cpu.InterruptEnableFlag;
            keyboard = new Keyboard(intSource);
            memory = cpu.Memory;
            videoMemory = new VideoMemory(memory);
            router.Add(drive).Add(keyboard).Add(outputLatch).Add(rom).Add(videoMemory);
            fileIo = FileIO.GetDefaultImplementation();
            fileHandler = new FileHandler(fileIo, memory, videoMemory);

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string fileName;

            if (File.Exists(romFilename))
            {
                fileName = romFilename;
            } else
            {
                fileName = placeHolderRomFilename;
            }

            if (!cpuIsSetup)
            {

                graphicsPainter = new GraphicsPainter(pictureBox1, videoMemory.Content, outputLatch, 0, 15);

                mem = new MemUtils(memory);
                tracer = new InstructionTracer(cpu);

                var program = fileIo.ReadFile(fileName);
                memory.SetContents(0, program);

                //set events
                cpu.AfterInstructionExecution += Z80OnAfterInstructionExecution;

                cpu.MemoryAccess += OnCpuBusAccess;
                cpu.PortAccess += OnCpuBusAccess;

                cpu.Reset();
                StartCpuTask();

                cpuIsSetup = true;

            }
            else {
                var program = fileIo.ReadFile(fileName);
                memory.SetContents(0, program);

                if (cpu.IsHalted)
                    cpu.Reset();
                else
                    resetting = true;
            }

            if (interruptTimer == null)
            {
                var timer = new Timer();
                timer.Interval = 10; //ms
                timer.Tick += (v, a) => intSource.IsEnabled = true;
                timer.Start();
                interruptTimer = timer;
            }

            if (watchTimer == null)
            {
                watchTimer = new Timer();
                watchTimer.Interval = 500; //ms
                watchTimer.Tick += (s, a) => {
                    lblFps.Text = graphicsPainter.CurrentFps;
                    lblInstructionsPerSecond.Text = (instructionsPerSecond / (watchTimer.Interval / 1000.0)).ToString();
                    instructionsPerSecond = 0;
                };
                watchTimer.Start();
            }

            if (debugTimer == null)
            {
                debugTimer = new Timer();
                debugTimer.Interval = 500; //ms
                debugTimer.Tick += (s, a) => {
                    if (!txtMCStart.Focused)
                    {
                        var value = mem.GetWordAtAddress(VzConstants.UserExecWordPtr);
                        txtMCStart.Text = string.Format("0x{0:X}", value);
                    }
                    if (!txtMCEnd.Focused)
                    {
                        txtMCEnd.Text = string.Format("0x{0:X}", EndMCProgram);
                    }
                };
                debugTimer.Start();
            }

            btnStart.Text = "Reset";
            btnPause.Enabled = true;
            btnContinue.Enabled = true;
        }

        private string GetTextModeText()
        {

            var sb = new StringBuilder();
            var offset = 0;
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 32; x++)
                {
                    //get value
                    ushort pos;
                    unchecked
                    {
                        pos = (ushort)(offset + y * 32 + x);
                    }
                    var value = videoMemory.Content[pos];

                    //create string
                    if (value < 0x20) value += 0x40;
                    if (value >= 0x60 && value < 0x80) value -= 0x40;
                    if (value > 32 && value <= 128)
                    {
                        sb.Append((char)value);
                    } else if (value == 32 && videoMemory.Content[pos] == 32) //inverted space, cursor
                    {
                        sb.Append('#');
                    }
                    {
                        sb.Append(' ');
                    }
                }
                sb.AppendLine("");
            }

            return sb.ToString();
        }
        private void SaveRegistersToMemory(ICpu z80)
        {
            ushort nextAddress = mem.SaveRegistersToMemory(z80.Registers, 0);
            z80.Memory[nextAddress] = outputLatch.Value;
        }
        private void LoadRegistersFromMemory(ICpu z80)
        {
            ushort nextAddress = mem.LoadRegistersFromMemory(z80.Registers, 0);
            outputLatch.Value = z80.Memory[nextAddress];
        }

        private void Z80OnAfterInstructionExecution(object sender, InstructionEventArgs args)
        {

            var z80 = (ICpu)sender;

            instructionsPerSecond += 1;

            if ( resetting)
            {
                cpu.Reset();
                resetting = false;
            }
           
            //reset INT on EI (enable interrupts)
            if (memory[z80.Registers.PC] == 0xFB)
            {
                intSource.IsEnabled = false;
            }

            if (savingImage)
            {
                SaveImage(ImageFilename);
                savingImage = false;
            }

            if (loadingImage)
            {
                loadingImage = false;
                args.StopWhenComplete=true;

            }

            if (isTrace)
                tracer.TraceNextInstruction();

        }

        HashSet<int> addresses = new HashSet<int>();
        private void OnCpuBusAccess(object sender, BusEventArgs args)
        {
            if (args.IsMemoryAccess)
            {
                if (args.IsRead)
                {
                    byte? value = router.HandleMemoryRead(args.Address);
                    if (value.HasValue)
                    {
                        args.Value = value.Value;
                        args.IsComplete = true;
                    }
                }
                else
                {
                    args.IsComplete = router.HandleMemoryWrite(args.Address, args.Value);
                }
            } else {
                if (args.IsRead)
                {
                    byte? value = router.HandlePortRead(args.Address);
                    if (value.HasValue)
                    {
                        args.Value = value.Value;
                        args.IsComplete = true;
                    }
                }
                else
                {
                    router.HandlePortWrite(args.Address, args.Value);
                }
            }
        }
        
        private void btnPause_Click(object sender, EventArgs e)
        {
            cpu.Pause();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            StartCpuTask();
        }

        private void StartCpuTask()
        {
            if (cpu != null)
                Task.Run(() => cpu.Continue());
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            keyboard.currentKey = e.KeyValue;
            keyboard.currentKeyCode = e.KeyData;
            e.Handled = true;

            //trigger interrupt early to handle keys fast
            intSource.IsEnabled = true;
        }
        private void frmMain_KeyUp(object sender, KeyEventArgs e)
        {
            keyboard.currentKey = null;
            keyboard.currentKeyCode = e.KeyCode ^ e.KeyData;
            e.Handled = true;
        }

        private void Button_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void btnTrace_Click(object sender, EventArgs e)
        {
            isTrace = !isTrace;
        }

        private void btnExec_Click(object sender, EventArgs e)
        {
            var addr = mem.GetWordAtAddress(VzConstants.UserExecWordPtr);
            cpu.ExecuteCall(addr);
        }

        private void btnSaveBasic_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var StartAddr = mem.GetWordAtAddress(VzConstants.StartBasicProgramPtr);
                var EndAddr = mem.GetWordAtAddress(VzConstants.EndBasicProgramPtr);
                fileHandler.SaveFile(VzConstants.FileTypeBasic, dlg.FileName, StartAddr, EndAddr);
            }
        }
        private void btnSaveMC_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var StartAddr = mem.GetWordAtAddress(VzConstants.UserExecWordPtr);
                var EndAddr = EndMCProgram;
                fileHandler.SaveFile(VzConstants.FileTypeMC, dlg.FileName, StartAddr, EndAddr);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                fileHandler.LoadFile(dlg.FileName);
            }
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            var frm = new frmDebug(cpu, mem);
            frm.Show();

            var frm2 = new frmMemoryView(cpu);
            frm2.Show();

        }

        private void txtMCStart_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = ValidateDebugTextbox(sender as Control);
        }

        private void txtMCEnd_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = ValidateDebugTextbox(sender as Control);
        }

        bool ValidateDebugTextbox(Control ctrl)
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
            mem.SetWordAtAddress(VzConstants.UserExecWordPtr, value);
        }

        private void txtMCEnd_Leave(object sender, EventArgs e)
        {
            EndMCProgram = MemUtils.StringToUShort(txtMCEnd.Text);
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ImageFilename = dlg.FileName;
                savingImage = true;
            }
        }

        private void btnQuickSave_Click(object sender, EventArgs e)
        {
            ImageFilename = quickSaveFilename;
            savingImage = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadImage(dlg.FileName);
            }
        }
        private void btnQuickLoad_Click(object sender, EventArgs e)
        {
            LoadImage(quickSaveFilename);
        }

        private void LoadImage(string fileName)
        {
            loadingImage = true;

            while (cpu.State == CpuState.Running)
                System.Threading.Thread.Sleep(0);

            var z80 = cpu;
            //Read image
            var image = fileIo.ReadFile(fileName);
            var rom = fileIo.ReadFile(romFilename);
            memory.SetContents(0, image); //todo dont read in the first 256 bytes, these are used for storing register since they are ROM in the VZ anyway

            z80.Reset();

            //Set register values
            LoadRegistersFromMemory(z80);

            //re-load rom
            z80.Memory.SetContents(0, rom, 0, 256);

            //Refresh video memory
            videoMemory.UpdateVideoMemoryFromMainMemory();

            StartCpuTask();
        }

        private void SaveImage(string fileName)
        {
            //Save registers to memory
            SaveRegistersToMemory(cpu);
            LoadRegistersFromMemory(cpu);

            //Write memory to file
            fileIo.WriteFile(ImageFilename, memory.GetContents(0, 0xFFFF));

             //re-read rom file to replace locations written with register values
            var rom = fileIo.ReadFile(romFilename);
            memory.SetContents(0, rom);
        }

        private void btnGR_Click(object sender, EventArgs e)
        {
            outputLatch.Value ^= (byte)VzConstants.OutputLatchBits.GraphicsMode; //toggle graphics mode
        }

        private void btnSmooth_Click(object sender, EventArgs e)
        {
            graphicsPainter.UseSmoothing = !graphicsPainter.UseSmoothing;
        }

        private void btnScale_Click(object sender, EventArgs e)
        {
            graphicsPainter.UseFixedScale = !graphicsPainter.UseFixedScale;
            graphicsPainter.FixedScale = 2;
        }

        private void btnSaveDisk_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog()==DialogResult.OK)
            {
                drive.SaveDiskImage(saveFileDialog.FileName);
            }
        }

        private void btnLoadDisk_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                drive.LoadDiskImage(openFileDialog.FileName);
            }
        }
    }
}
