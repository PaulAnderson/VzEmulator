using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VzEmulate2.Screen;
using VzEmulator.Debug;

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
        string romFilename = "Roms/VZ300.Rom";
        bool isTrace;
        long instructionsPerSecond;
        private MemUtils mem;
        InterruptSource intSource = new InterruptSource() ;
        InstructionTracer tracer;
        ushort EndMCProgram;
        GraphicsPainter graphicsPainter;

        byte _outputLatch = 0;
        public byte OutputLatch {
            get
            {
                return _outputLatch;
            }
            set
            {
                _outputLatch = value;
                graphicsPainter?.SetModeFromOutputLatch(value);
            }
        }

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
        }

        private void button1_Click(object sender, EventArgs e)
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
                VideoMemory = new byte[0x0800];
                graphicsPainter = new GraphicsPainter(pictureBox1, VideoMemory, 0, 25);

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
                watchTimer.Tick += (s,a) => {
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

            button1.Text = "Reset";
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
                    if (value>32 && value <= 128)
                    {
                        sb.Append((char)value);
                    } else if (value==32 && VideoMemory[pos]==32) //inverted space, cursor
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
            z80.Memory[24] = OutputLatch;
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
            OutputLatch = z80.Memory[24];

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
                savingImage = false;

                //Save registers to memory
                SaveRegistersToMemory(z80);
                LoadRegistersFromMemory(z80);

                //Write memory to file
                File.WriteAllBytes(ImageFilename, z80.Memory.GetContents(0, 0xFFFF));

                //re-read rom file to replace locations written with register values
                var rom = File.ReadAllBytes(romFilename);
                z80.Memory.SetContents(0, rom);

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
            if (args.EventType == MemoryAccessEventType.BeforeMemoryWrite)
            {
                if (args.Value == 19 && cpu.Memory[args.Address] == 20)
                {
                    if (!addresses.Contains(args.Address))
                    {
                        addresses.Add(args.Address);
                        Console.WriteLine(args.Address.ToString("X"));
                    }
                }
            }
            if (args.Address >= 0x6800 && args.Address <= 0x6FFF && args.EventType == MemoryAccessEventType.AfterMemoryWrite)
            {
                var oldValue = OutputLatch;
                OutputLatch = args.Value;
            }
            if (args.EventType == MemoryAccessEventType.AfterPortWrite)
            {
                var addr = args.Address;
                var value = args.Value;
            }
                //Block writes to ROM
                if (args.Address < 0x6000 && args.EventType == MemoryAccessEventType.BeforeMemoryWrite)
                args.CancelMemoryAccess = true;

            //Keyboard
            if (args.EventType == MemoryAccessEventType.AfterMemoryRead && args.Address >= 0x6800 && args.Address < 0x7000)
            {
                args.CancelMemoryAccess = true;
                var addr = args.Address & 0xff;
                addr = 0xff - addr;
                args.Value = 0b10111111;

                if (currentKey.HasValue || currentKeyCode != Keys.None)
                {
                    var keyCode = currentKeyCode & Keys.KeyCode;

                    if (currentKey >= 97 && currentKey <= 122)
                        currentKey -= 32;
                    var c = '\0';
                    c = currentKey.HasValue ? (char)currentKey.Value : '\0';

                    if ((addr & 1)>0) {
                        if (c == 'R')
                            args.Value &= 0b11011111;
                        if (c == 'Q')
                            args.Value &= 0b11101111;
                        if (c == 'E')
                            args.Value &= 0b11110111;
                        if (c == ' ')
                            args.Value &= 0b11111011;
                        if (c == 'W')
                            args.Value &= 0b11111101;
                        if (c == 'T')
                            args.Value &= 0b11111110;
                       }
                    if ((addr & 2)>0) {
                        if (c == 'F')
                            args.Value &= 0b11011111;
                        if (c == 'A')  
                            args.Value &= 0b11101111;
                        if (c == 'D')  
                            args.Value &= 0b11110111;
                        if ((currentKeyCode & Keys.Control)==Keys.Control
                            || keyCode == Keys.Left || keyCode == Keys.Back
                            || keyCode == Keys.Right || keyCode == Keys.Up
                            || keyCode == Keys.Down || keyCode == Keys.Insert
                            || keyCode == Keys.Delete || keyCode == Keys.End )
                            args.Value &= 0b11111011;
                        if (c == 'S')  
                            args.Value &= 0b11111101;
                        if (c == 'G')  
                            args.Value &= 0b11111110;
                    }
                    if ((addr & 4) > 0)
                    {
                        if (c == 'V')
                            args.Value &= 0b11011111;
                        if (c == 'Z')
                            args.Value &= 0b11101111;
                        if (c == 'C')
                            args.Value &= 0b11110111;
                        if ((currentKeyCode & Keys.Shift) == Keys.Shift || 
                            ((currentKeyCode & Keys.Control) != Keys.Control && (currentKeyCode & Keys.Shift) != Keys.Shift && (currentKeyCode & Keys.KeyCode) == Keys.Oemplus))
                            args.Value &= 0b11111011;
                        if (c == 'X')
                            args.Value &= 0b11111101;
                        if (c == 'B')
                            args.Value &= 0b11111110;
                    }
                    if ((addr & 8) > 0)
                    {
                        if (c == '4')
                            args.Value &= 0b11011111;
                        if (c == '1')
                            args.Value &= 0b11101111;
                        if (c == '3')
                            args.Value &= 0b11110111;
                        if ((currentKeyCode & Keys.Alt) == Keys.Alt)
                            args.Value &= 0b11111011;
                        if (c == '2')
                            args.Value &= 0b11111101;
                        if (c == '5')
                            args.Value &= 0b11111110;
                    }
                    if ((addr & 16) > 0)
                    {
                        if (c == 'M' || keyCode == Keys.Left || keyCode == Keys.Back)
                            args.Value &= 0b11011111;
                        if (c == ' ' || keyCode == Keys.Down)
                            args.Value &= 0b11101111;
                        if (keyCode == Keys.Oemcomma 
                            || keyCode == Keys.Right)
                            args.Value &= 0b11110111;
                        if (keyCode == Keys.F1)
                            args.Value &= 0b11111011;
                        if (keyCode == Keys.OemPeriod
                            || keyCode == Keys.Up)
                            args.Value &= 0b11111101;
                        if (c == 'N')
                            args.Value &= 0b11111110;
                    }
                    if ((addr & 32) > 0)
                    {
                        if (c == '7')
                            args.Value &= 0b11011111;
                        if (c == '0')
                            args.Value &= 0b11101111;
                        if (c == '8')
                            args.Value &= 0b11110111;
                        if (c == '-' || (currentKeyCode & Keys.KeyCode) == Keys.OemMinus || ((currentKeyCode & Keys.Shift)!=Keys.Shift & (currentKeyCode & Keys.KeyCode) == Keys.Oemplus))
                            args.Value &= 0b11111011;
                        if (c == '9')
                            args.Value &= 0b11111101;
                        if (c == '6')
                            args.Value &= 0b11111110;
                    }
                    if ((addr & 64) > 0)
                    {
                        if (c == 'U')
                            args.Value &= 0b11011111;
                        if (c == 'P')
                            args.Value &= 0b11101111;
                        if (c == 'I')
                            args.Value &= 0b11110111;
                        if (c == (char)13)
                            args.Value &= 0b11111011;
                        if (c == 'O')
                            args.Value &= 0b11111101;
                        if (c == 'Y')
                            args.Value &= 0b11111110;
                    }
                    if ((addr & 128) > 0)
                    {
                        if (c == 'J')
                            args.Value &= 0b11011111;
                        if (c == ';' || keyCode == Keys.Delete || keyCode == Keys.Oem1 ||
                            ((currentKeyCode & Keys.Shift) == Keys.Shift && (currentKeyCode & Keys.KeyCode) == Keys.Oemplus))
                            args.Value &= 0b11101111;
                        if (c == 'K')
                            args.Value &= 0b11110111;
                        if (c == ':' || keyCode == Keys.End || keyCode == Keys.Oem7) 
                            args.Value &= 0b11111011;
                        if (c == 'L' || keyCode == Keys.Insert)
                            args.Value &= 0b11111101;
                        if (c == 'H')
                            args.Value &= 0b11111110;
                    }

                } else
                {
                    //keys scanned but no key pressed. Good place for a small delay to reduce host cpu use when then guest process is just waiting for input
                    System.Threading.Thread.Sleep(TimeSpan.Zero); //Do any other work waiting
                }

                if (intSource.IntActive)
                {
                    args.Value &= 0x7f;
                    intSource.IntActive = false ;
                }
            }

            //Display
            if (args.EventType == MemoryAccessEventType.AfterMemoryWrite &&  args.Address >= 0x7000 && args.Address <= 0x77FF)
            {
                var pos = (args.Address - 0x7000);
                VideoMemory[pos] = args.Value;

                //todo
                var y = pos / 32;
                var x = pos - y * 32;
            }
        }
        private void UpdateVideoMemoryFromMainMemory()
        {
            for (int i=0x7000;i<=0x77FF;i++)
            {
                VideoMemory[i - 0x7000] = cpu.Memory[i];
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

        int? currentKey;
        Keys? currentKeyCode = Keys.None;

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            currentKey = e.KeyValue;
            currentKeyCode = e.KeyData;
            e.Handled = true;

            //trigger interrupt early to handle keys fast
            intSource.IntActive = true;
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            currentKey = null;
            currentKeyCode = e.KeyCode ^ e.KeyData;
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
                SaveFile(VzConstants.FileTypeBasic,dlg.FileName, StartAddr,EndAddr);
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
                    cpu.Memory[VzConstants.UserExecWordPtr+1] = file.startaddr_h;

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

            //Refresh video memory in case the loaded file overlapped with video memory. Todo check this and only update if needed
            UpdateVideoMemoryFromMainMemory();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var frm = new frmDebug(cpu,mem);
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

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            pictureBox1.Visible = !pictureBox1.Visible;
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            pictureBox1.Visible = !pictureBox1.Visible;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK )
            {
                ImageFilename = dlg.FileName;
                savingImage = true;
            }
        }

        private void btnQuickSave_Click(object sender, EventArgs e)
        {
            ImageFilename = "QuickSave1.img";
            savingImage = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ImageFilename = dlg.FileName;
                LoadImage();
            }
        }
        private void btnQuickLoad_Click(object sender, EventArgs e)
        {
            ImageFilename = "QuickSave1.img";
            LoadImage();
        }
        private void LoadImage()
        {
            loadingImage = true;

            while (cpu.State == ProcessorState.Running)
                System.Threading.Thread.Sleep(0);

            var z80 = cpu;
            //Read image
            var image = File.ReadAllBytes(ImageFilename);
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

        private void btnGR_Click(object sender, EventArgs e)
        {
            OutputLatch ^= 0x08; //toggle graphics mode
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
    }
}
;