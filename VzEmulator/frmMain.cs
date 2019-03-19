using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VzEmulate2.Screen;
using VzEmulator.Debug;
using VzEmulator.Peripherals;

namespace VzEmulate2
{
    public partial class frmMain : Form
    {
        private Z80Processor cpu;
        private byte[] VideoMemory;
        Timer interruptTimer;
        Timer watchTimer;
        Timer debugTimer;
        bool stopping;
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
        InterruptSource intSource = new InterruptSource();
        InstructionTracer tracer;
        ushort EndMCProgram;
        GraphicsPainter graphicsPainter;
        PeripheralRouter router = new PeripheralRouter();
        Drive drive = new Drive();
        Keyboard keyboard;
        OutputLatch outputLatch = new OutputLatch();

        public frmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in pnlTop.Controls)
            {
                ctrl.PreviewKeyDown += Button_PreviewKeyDown;
            }

            SetupDevices();
        }

        private void SetupDevices()
        {
            keyboard = new Keyboard(intSource);
            router.Add(drive).Add(keyboard).Add(outputLatch);
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

            if (cpu == null)
            {
                VideoMemory = new byte[VzConstants.VideoRamSize+1];
                graphicsPainter = new GraphicsPainter(pictureBox1, VideoMemory, outputLatch, 0, 25);

                var z80 = new Z80Processor();

                cpu = z80;
                z80.AutoStopOnDiPlusHalt = false;
                z80.AutoStopOnRetWithStackEmpty = false;

                z80.ClockSynchronizer = null;
                z80.RegisterInterruptSource(intSource);

                mem = new MemUtils(z80.Memory);
                tracer = new InstructionTracer(z80);

                var program = File.ReadAllBytes(fileName);
                z80.Memory.SetContents(0, program);

                //set events
                z80.BeforeInstructionFetch += Z80OnBeforeInstructionFetch;
                z80.AfterInstructionExecution += Z80OnAfterInstructionExecution;

                z80.MemoryAccess += z80OnMemoryAccess;

                cpu.Reset();
                StartCpuTask();
            } else {
                var program = File.ReadAllBytes(fileName);
                cpu.Memory.SetContents(0, program);

                if (cpu.IsHalted)
                    cpu.Reset();
                else
                    resetting = true;
            }

            if (interruptTimer == null)
            {
                var timer = new Timer();
                timer.Interval = 10; //ms
                timer.Tick += (v, a) => intSource.IntActive = true;
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
                    var value = VideoMemory[pos];

                    //create string
                    if (value < 0x20) value += 0x40;
                    if (value >= 0x60 && value < 0x80) value -= 0x40;
                    if (value > 32 && value <= 128)
                    {
                        sb.Append((char)value);
                    } else if (value == 32 && VideoMemory[pos] == 32) //inverted space, cursor
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
        private void SaveRegistersToMemory(IZ80Processor z80)
        {
            z80.Memory[0] = z80.Registers.A;
            z80.Memory[1] = z80.Registers.F;
            z80.Memory[2] = z80.Registers.B;
            z80.Memory[3] = z80.Registers.C;
            z80.Memory[4] = z80.Registers.D;
            z80.Memory[5] = z80.Registers.E;
            z80.Memory[6] = z80.Registers.H;
            z80.Memory[7] = z80.Registers.L;
            z80.Memory[8] = z80.Registers.IXH;
            z80.Memory[9] = z80.Registers.IXL;
            z80.Memory[10] = z80.Registers.IYH;
            z80.Memory[11] = z80.Registers.IYL;
            z80.Memory[12] = (byte)((z80.Registers.SP & 0xFF00) >> 8);
            z80.Memory[13] = (byte)(z80.Registers.SP & 0xFF);
            z80.Memory[14] = (byte)((z80.Registers.PC & 0xFF00) >> 8);
            z80.Memory[15] = (byte)(z80.Registers.PC & 0xFF);
            z80.Memory[16] = z80.Registers.Alternate.A;
            z80.Memory[17] = z80.Registers.Alternate.F;
            z80.Memory[18] = z80.Registers.Alternate.B;
            z80.Memory[19] = z80.Registers.Alternate.C;
            z80.Memory[20] = z80.Registers.Alternate.D;
            z80.Memory[21] = z80.Registers.Alternate.E;
            z80.Memory[22] = z80.Registers.Alternate.H;
            z80.Memory[23] = z80.Registers.Alternate.L;
            z80.Memory[24] = outputLatch.Value;
        }
        private void LoadRegistersFromMemory(IZ80Processor z80)
        {
            z80.Registers.A = z80.Memory[0];
            z80.Registers.F = z80.Memory[1];
            z80.Registers.B = z80.Memory[2];
            z80.Registers.C = z80.Memory[3];
            z80.Registers.D = z80.Memory[4];
            z80.Registers.E = z80.Memory[5];
            z80.Registers.H = z80.Memory[6];
            z80.Registers.L = z80.Memory[7];
            z80.Registers.IXH = z80.Memory[8];
            z80.Registers.IXL = z80.Memory[9];
            z80.Registers.IYH = z80.Memory[10];
            z80.Registers.IYL = z80.Memory[11];
            z80.Registers.SP = (short)(z80.Memory[12] << 8);
            z80.Registers.SP += z80.Memory[13];
            z80.Registers.PC = (ushort)(z80.Memory[14] << 8);
            z80.Registers.PC += z80.Memory[15];
            z80.Registers.Alternate.A = z80.Memory[16];
            z80.Registers.Alternate.F = z80.Memory[17];
            z80.Registers.Alternate.B = z80.Memory[18];
            z80.Registers.Alternate.C = z80.Memory[19];
            z80.Registers.Alternate.D = z80.Memory[20];
            z80.Registers.Alternate.E = z80.Memory[21];
            z80.Registers.Alternate.H = z80.Memory[22];
            z80.Registers.Alternate.L = z80.Memory[23];
            outputLatch.Value = z80.Memory[24];

        }

        private void Z80OnAfterInstructionExecution(object sender, AfterInstructionExecutionEventArgs args)
        {

            var z80 = (IZ80Processor)sender;

            instructionsPerSecond += 1;

            if (stopping)
            {
                args.ExecutionStopper.Stop(true);
                stopping = false;
            }

            if (resetting)
            {
                cpu.Reset();
                resetting = false;
            }
        }
        private void Z80OnBeforeInstructionFetch(object sender, BeforeInstructionFetchEventArgs args)
        {
            var z80 = (IZ80Processor)sender;

            //reset INT on EI (enable interrupts)
            if (z80.Memory[z80.Registers.PC] == 0xFB)
            {
                intSource.IntActive = false;
            }

            if (savingImage)
            {
                SaveImage(ImageFilename);
                savingImage = false;
            }

            if (loadingImage)
            {
                loadingImage = false;

                args.ExecutionStopper.Stop(true);

            }

            if (isTrace)
                tracer.TraceNextInstruction();

        }

        HashSet<int> addresses = new HashSet<int>();
        private void z80OnMemoryAccess(object sender, MemoryAccessEventArgs args)
        {
            if (args.EventType == MemoryAccessEventType.AfterPortWrite)
            {
                router.HandlePortWrite(args.Address, args.Value);
            }

            if (args.EventType == MemoryAccessEventType.BeforePortRead)
            {

                byte? value = router.HandlePortRead(args.Address);
                if (value.HasValue)
                {
                    args.Value = value.Value;
                    args.CancelMemoryAccess = true;
                }
            }

            if (args.EventType == MemoryAccessEventType.BeforeMemoryRead)
            {
                byte? value = router.HandleMemoryRead(args.Address);
                if (value.HasValue)
                {
                    args.Value = value.Value;
                    args.CancelMemoryAccess = true;
                }
            }

            if (args.EventType == MemoryAccessEventType.BeforeMemoryWrite)
            {
                args.CancelMemoryAccess = router.HandleMemoryWrite(args.Address, args.Value);
            }

            //Block writes to ROM
            if (args.Address < VzConstants.TopOfRom && args.EventType == MemoryAccessEventType.BeforeMemoryWrite)
                args.CancelMemoryAccess = true;

            //Display
            if (args.EventType == MemoryAccessEventType.AfterMemoryWrite && args.Address >= VzConstants.VideoRamStart && args.Address <= VzConstants.VideoRamEnd)
            {
                var pos = (args.Address & VzConstants.VideoRamSize);
                VideoMemory[pos] = args.Value;

                //todo
                var y = pos / 32;
                var x = pos - y * 32;
            }
        }
        private void UpdateVideoMemoryFromMainMemory()
        {
            for (int i = VzConstants.VideoRamStart; i <= VzConstants.VideoRamEnd; i++)
            {
                VideoMemory[i - VzConstants.VideoRamStart] = cpu.Memory[i];
            }
        }
        private void btnPause_Click(object sender, EventArgs e)
        {
            stopping = true;
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            StartCpuTask();
        }

        private void StartCpuTask()
        {
            stopping = false;
            if (cpu != null)
                Task.Run(() => cpu.Continue());
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            keyboard.currentKey = e.KeyValue;
            keyboard.currentKeyCode = e.KeyData;
            e.Handled = true;

            //trigger interrupt early to handle keys fast
            intSource.IntActive = true;
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
                SaveFile(VzConstants.FileTypeBasic, dlg.FileName, StartAddr, EndAddr);
            }
        }
        private void btnSaveMC_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var StartAddr = mem.GetWordAtAddress(VzConstants.UserExecWordPtr);
                var EndAddr = EndMCProgram;
                SaveFile(VzConstants.FileTypeMC, dlg.FileName, StartAddr, EndAddr);
            }
        }

        private void SaveFile(byte FileType, string Filename, ushort StartAddress, ushort EndAddress)
        {
            var length = EndAddress - StartAddress;
            var file = new VzFile(length);
            file.header = 0x2020;
            file.fileType = FileType;
            unchecked
            {
                file.startaddr_h = (byte)((StartAddress & 0xFF00) >> 8);
                file.startaddr_l = (byte)(StartAddress & 0xFF);
            }
            for (int i = 0; i < file.content.Length - VzFile.ProgramContentStart; i++)
            {
                file.content[i + VzFile.ProgramContentStart] = cpu.Memory[StartAddress + i];
            }

            File.WriteAllBytes(Filename, file.content);

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadFile(dlg.FileName);
            }
        }
        private void LoadFile(string Filename)
        {
            var loader = new VzFileLoader();
            var file = loader.LoadFile(Filename);
            if (file.header == 0x2020 | file.header == 0x30465a56)
            {
                //valid vz file
                //get start address
                ushort addr = (ushort)file.startaddr_h;
                addr <<= 8;
                addr |= file.startaddr_l;
                ushort addrEnd;
                unchecked
                {
                    addrEnd = (ushort)(addr + file.content.Length - VzFile.ProgramContentStart);
                }

                //load content
                for (int i = 0; i < file.content.Length - VzFile.ProgramContentStart; i++)
                {
                    cpu.Memory[addr + i] = file.content[i + VzFile.ProgramContentStart];
                }

                //Save start address pointer
                if (file.fileType == VzConstants.FileTypeBasic)
                {
                    //basic file
                    cpu.Memory[VzConstants.StartBasicProgramPtr] = file.startaddr_l;
                    cpu.Memory[VzConstants.StartBasicProgramPtr + 1] = file.startaddr_h;

                }
                else if (file.fileType == VzConstants.FileTypeMC)
                {
                    //Machinecode file
                    cpu.Memory[VzConstants.UserExecWordPtr] = file.startaddr_l;
                    cpu.Memory[VzConstants.UserExecWordPtr + 1] = file.startaddr_h;

                    // cpu.ExecuteCall(addr);
                }
                else
                {
                    //unknown file type
                }
                //save end address pointer
                if (file.fileType == 0xF0)
                {
                    //basic file
                    cpu.Memory[0x78F9] = (byte)(addrEnd & 0x00FF);
                    cpu.Memory[0x78FA] = (byte)((addrEnd & 0xFF00) >> 8);
                    cpu.Memory[0x78FB] = (byte)(addrEnd & 0x00FF);
                    cpu.Memory[0x78FC] = (byte)((addrEnd & 0xFF00) >> 8);
                    cpu.Memory[0x78FD] = (byte)(addrEnd & 0x00FF);
                    cpu.Memory[0x78FE] = (byte)((addrEnd & 0xFF00) >> 8);
                }
            }
            else
            {
                //invalid file
            }

            //Refresh video memory in case the loaded file overlapped with video memory.
            UpdateVideoMemoryFromMainMemory();

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

            while (cpu.State == ProcessorState.Running)
                System.Threading.Thread.Sleep(0);

            var z80 = cpu;
            //Read image
            var image = File.ReadAllBytes(fileName);
            var rom = File.ReadAllBytes(romFilename);
            z80.Memory.SetContents(0, image); //todo dont read in the first 256 bytes, these are used for storing register since they are ROM in the VZ anyway

            z80.Reset();

            //Set register values
            LoadRegistersFromMemory(z80);

            //re-load rom
            z80.Memory.SetContents(0, rom, 0, 256);

            //Refresh video memory
            UpdateVideoMemoryFromMainMemory();

            StartCpuTask();
        }

        private void SaveImage(string fileName)
        {
            //Save registers to memory
            SaveRegistersToMemory(cpu);
            LoadRegistersFromMemory(cpu);

            //Write memory to file
            File.WriteAllBytes(ImageFilename, cpu.Memory.GetContents(0, 0xFFFF));

             //re-read rom file to replace locations written with register values
            var rom = File.ReadAllBytes(romFilename);
            cpu.Memory.SetContents(0, rom);
        }

        private void btnGR_Click(object sender, EventArgs e)
        {
            outputLatch.Value ^= 0x08; //toggle graphics mode
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
;