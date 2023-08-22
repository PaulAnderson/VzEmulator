using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using VzEmulator.Peripherals;
using VzEmulator.Screen;

namespace VzEmulator
{
    public class MachineRunner
    {
        private const string PlaceHolderRomFilename = "Roms/Placeholder.Rom";
        private string romFilename = "Roms/VZ300.ROM";
        private string dosRomFileName = "Roms/VZDOS.ROM";
        private bool dosRomPresent = true;
        private Timer watchTimer;
        private Timer debugTimer;

        private Machine _machine;

        private FileIO fileIo;
        private FileHandler fileHandler;
        private ImageGraphicsPainter graphicsPainter;

        public MachineRunner(Machine machine)
        {
            _machine = machine;

            fileIo = FileIO.GetDefaultImplementation();
            fileHandler = new FileHandler(fileIo, machine.Cpu.Memory, machine.VideoMemory);
        }

        public void EnableTrace(bool value)
        {
            _machine.TraceEnabled = value;
        }

        internal void Start(string userFileName)
        {

            string fileName;

            if (File.Exists(romFilename))
            {
                fileName = romFilename;
            }
            else
            {
                fileName = PlaceHolderRomFilename;
            }
            if (graphicsPainter == null)
            {
                graphicsPainter = new ImageGraphicsPainter(_machine.VideoMemory.Content, _machine.OutputLatch, 0, _machine.AuExtendedGraphicsLatch);
            }

            byte[] program = new byte[0x6000];
            var mainRom = fileIo.ReadFile(fileName);
            mainRom.CopyTo(program, 0);
            if (mainRom.Length<=0x4000 && File.Exists(dosRomFileName))
            {
                var dosRom = fileIo.ReadFile(dosRomFileName);
                dosRom.CopyTo(program, 0x4000);
            }
            if (!dosRomPresent)
            {
                //clear bytes from 0x4000 to 0x5fff
                for (int i = 0x4001; i < 0x6000; i++)
                {
                    program[i] = 0x00;
                }
            }
            _machine.ClockSynced = false;
            _machine.Setup(program);

            int targetCycleCount = 10000000;
            _machine.SetCyclesCallback(() =>
            {
               
                LatestImage = graphicsPainter.GetImage();

                
                //todo back up state, ready to load next program
            },targetCycleCount);

        }
        public Bitmap GetImage()
        {
            LatestImage = graphicsPainter.GetImage();
            return LatestImage;
        }
        internal void Pause()
        {
            _machine.Pause();
        }

        public void UnPause()
        {
            _machine.StartCpuTask();
        }

        internal void SaveBasicFile(string fileName)
        {
            var StartAddr = _machine.Memory.GetWordAtAddress(VzConstants.StartBasicProgramPtr);
            var EndAddr = _machine.Memory.GetWordAtAddress(VzConstants.EndBasicProgramPtr);
            fileHandler.SaveFile(VzConstants.FileTypeBasic, fileName, StartAddr, EndAddr);
        }

        internal void JumpToUsrExec()
        {
            _machine.JumpToUsrExec();
        }

        internal void DebugRegisters()
        {
            var frm = new frmDebug(_machine.Cpu, _machine.Memory);
            frm.Show();
        }

        internal void DebugMemory()
        {
            var frm2 = new frmMemoryView(_machine.Cpu.Memory);
            frm2.Show();
        }
        internal void EditDisk()
        {
            //todo select disk to edit
            var frm2 = new frmMemoryView(_machine.Drive.Disk0);
            frm2.Show();
        }

        internal void SetMemory( int startAddress, byte[] content)
        {
            _machine.Cpu.Memory.SetContents(startAddress, content);
        }
        public void LoadImage(string fileName)
        {
            _machine.StopCpuAfterNextInstruction();

            var z80 = _machine.Cpu;

            while (z80.State == CpuState.Running)
                System.Threading.Thread.Sleep(0);

            //Read image
            var image = fileIo.ReadFile(fileName);
            var rom = fileIo.ReadFile(romFilename);
            _machine.Cpu.Memory.SetContents(0, image); //todo don't read in the first 256 bytes, these are used for storing register since they are ROM in the VZ anyway

            //Set register values
            _machine.LoadRegistersFromMemory(z80);

            //re-load rom
            z80.Memory.SetContents(0, rom, 0, 256);

            //Refresh video memory
            _machine.VideoMemory.UpdateVideoMemoryFromMainMemory();

            _machine.StartCpuTask();
        }

        public void SaveImage(string fileName)
        {
            _machine.SetAfterInstructionExecutionCallback(() =>
            {
                //Save registers to memory
                _machine.SaveRegistersToMemory(_machine.Cpu);
                _machine.LoadRegistersFromMemory(_machine.Cpu);

                //Write memory to file
                fileIo.WriteFile(fileName, _machine.Cpu.Memory.GetContents(0, 0xFFFF));

                //re-read rom file to replace locations written with register values
                var rom = fileIo.ReadFile(romFilename);
                _machine.Cpu.Memory.SetContents(0, rom);
            });
        }

        public byte[] LatestSnapshot;
        public void GetSnapshot()
        {
            LatestSnapshot = null;

            if (_machine.Cpu.State == CpuState.Running)
            {
                _machine.SetAfterInstructionExecutionCallback(() =>
                {
                    DoSnapshot();
                });
            } else
            {
                DoSnapshot();
            }
            void DoSnapshot()
            {
                //Write memory to file
                var memorySize = 0x10000; //64k. todo extended video memory
                var machineState = _machine.GetRegistersAndMachineState(_machine.Cpu);
                var memory = new Byte[memorySize + machineState.Length];

                _machine.Cpu.Memory.GetContents(0, 0x10000).CopyTo(memory, machineState.Length - 1); //Shift memory by size of machine state
                machineState.CopyTo(memory, 0); //copy machine state to start of memory array
                LatestSnapshot = memory;
            }
        }

        public void ApplySnapshot(byte[] snapShot)
        {
            _machine.StopCpuAfterNextInstruction();

            var z80 = _machine.Cpu;

            while (z80.State == CpuState.Running)
                System.Threading.Thread.Sleep(0);

            //Read image
            var snapshotSize = _machine.ApplyRegistersAndMachineState(_machine.Cpu, snapShot);
            _machine.Cpu.Memory.SetContents(0, new byte[0x10000]); //clear memory
            _machine.Cpu.Memory.SetContents(0, snapShot, snapshotSize-1, 0x10000);

            //Refresh video memory
            _machine.VideoMemory.UpdateVideoMemoryFromMainMemory();

            _machine.StartCpuTask();
        }


        public void KeyDown(int keyValue, Keys keyData)
        {
            _machine.Keyboard.SetKeyState(new Keyboard.KeyState(keyValue, keyData));

            //trigger interrupt early to handle keys fast
            //_machine.IntSource.IsEnabled = true;
        }
        public void KeyUp(Keys keyData)
        {
            _machine.Keyboard.SetKeyState(new Keyboard.KeyState(null, keyData));
        }

        public void SaveMcFile(string fileName)
        {
            var StartAddr = StartMcProgram;
            var EndAddr = EndMCProgram;
            fileHandler.SaveFile(VzConstants.FileTypeMC, fileName, StartAddr, EndAddr);
        }

        public ushort EndMCProgram { get; set; }
        public ushort StartMcProgram {
            get
            {
                return _machine.Memory.GetWordAtAddress(VzConstants.UserExecWordPtr);
            }
            set
            {
                _machine.Memory.SetWordAtAddress(VzConstants.UserExecWordPtr, value);
            }
        }

        public bool SoundEnabled { set {
                _machine.SoundEnabled = value;
            } }
        public bool CassetteSoundStreamEnabled { set
            {
                _machine.SoundOutput.MixStream2 = value;
            } }
        public bool SoundTestTone { set {
                _machine.SoundTestTone = value;
            } }

        public Bitmap LatestImage { get;   set; }

        public void LoadAndRunFile(string filename)
        {
            if (filename == null)
                return;

            _machine.Cpu.Memory[VzConstants.StartBasicProgramPtr] = 0;
            _machine.Cpu.Memory[VzConstants.StartBasicProgramPtr + 1] = 0;
            _machine.Cpu.Memory[VzConstants.UserExecWordPtr] = 0;
            _machine.Cpu.Memory[VzConstants.UserExecWordPtr + 1] = 0;

            var address= LoadFile(filename);

            Run(address.Start);

        }
        public void Run(ushort address, bool setBasicPointer = false)
        {
            if (setBasicPointer)
            {
                _machine.Cpu.Memory[VzConstants.StartBasicProgramPtr] = (byte)((address & 0xFF00) >> 8);
                _machine.Cpu.Memory[VzConstants.StartBasicProgramPtr + 1] = (byte)(address & 0xFF);

            }
            if (_machine.Cpu.Memory[VzConstants.StartBasicProgramPtr] > 0)
            {
                _machine.Cpu.Registers.AF = 0x44;
                _machine.Cpu.Registers.BC = 0x1D1E;
                _machine.Cpu.Registers.DE = (short)(address - 1);//31464 0x7AE8;
                _machine.Cpu.Registers.HL = (short)address;//31465 0x7AE9;
                _machine.Cpu.Registers.IX = 0;
                _machine.Cpu.Registers.SP = unchecked((short)0xFE94);
                _machine.Cpu.Registers.AltAF = 0;
                _machine.Cpu.Registers.AltBC = 0;
                _machine.Cpu.Registers.AltDE = 7515;
                _machine.Cpu.Registers.AltHL = 0;
                _machine.Cpu.Registers.PC = 0x1D37;
                _machine.Cpu.InterruptEnableFlag.IsEnabled = false;
                _machine.Cpu.Registers.IFF1 = true;
                    //_machine.Call(0x1D37);
                //todo ensure correct values in registers based on loaded program address
                //todo first load always errors. 2nd works
            }
            else
            {
                _machine.Cpu.Registers.PC = (ushort)((_machine.Cpu.Memory[VzConstants.UserExecWordPtr + 1] << 8) +
                                                _machine.Cpu.Memory[VzConstants.UserExecWordPtr]);

            }
            //_machine.Call(0x1D37); //todo stop on return, change to run program call
            if (_machine.Cpu.State != CpuState.Running)
                _machine.StartCpuTask();

            _machine.SetCyclesCallback(() =>
            {
                _machine.Pause();

                //Get image
                LatestImage = graphicsPainter.GetImage();
            }, 3540000);
        }
        public void run1s()
        {
            LatestImage = null;

            if (_machine.Cpu.State != CpuState.Running)
                _machine.StartCpuTask();

            _machine.SetCyclesCallback(() =>
                {
                _machine.Pause();

                    //Get image
                    LatestImage = graphicsPainter.GetImage();
                }, 3540000);
                   }

        public AddressRange LoadFile(string fileName)
        {
            var addressRange = fileHandler.LoadFile(fileName);
            StartMcProgram = addressRange.Start;
            EndMCProgram = addressRange.End;
            return addressRange;
        }

        public void SaveDiskImage(string fileName, bool reFormat=false)
        {
            _machine.Drive.SaveDiskImage(fileName, reFormat);
        }

        public void LoadDiskImage(string fileName)
        {
            _machine.Drive.LoadDiskImage(fileName);
        }

        internal IPrinterOutput GetPrinterOutput()
        {
            return _machine.PrinterOutput;
        }
        internal IAudioOutput GetSoundOutput()
        {
            return _machine.SoundOutput;
        }

        internal void StartRecordingCassette(string fileName)
        {
            _machine.SoundOutput.StartRecordStream2(fileName);
        }
        internal void StartPlayingCassette(string fileName)
        {
            //open file for reading using a stream
            _machine.SoundInput.InputStream= new FileStream(fileName, FileMode.Open,FileAccess.Read);
        }

        internal void SetDosRomPresent(bool isPresent)
        {
            dosRomPresent = isPresent;
        }
          
    }
}
