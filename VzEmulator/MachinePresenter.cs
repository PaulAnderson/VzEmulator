using System;
using System.IO;
using System.Windows.Forms;
using VzEmulator.Peripherals;
using VzEmulator.Screen;

namespace VzEmulator
{
    public class MachinePresenter
    {
        private const string PlaceHolderRomFilename = "Roms/Placeholder.Rom";
        private string romFilename = "Roms/ROM-FULL.DBG";

        private Timer interruptTimer;
        private Timer watchTimer;
        private Timer debugTimer;

        private Machine _machine;
        private IMachineView _view;

        private FileIO fileIo;
        private FileHandler fileHandler;
        private GraphicsPainter graphicsPainter;

        public MachinePresenter(Machine machine, IMachineView view)
        {
            _machine = machine;
            _view = view;

            fileIo = FileIO.GetDefaultImplementation();
            fileHandler = new FileHandler(fileIo, machine.Cpu.Memory, machine.VideoMemory);
        }

        public void EnableTrace(bool value)
        {
            _machine.TraceEnabled = value;
        }

        internal void Start()
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

            graphicsPainter = new GraphicsPainter(_view.RenderControl, _machine.VideoMemory.Content, _machine.OutputLatch, 0, 33);
            var program = fileIo.ReadFile(fileName);
            _machine.Setup(program);

            if (interruptTimer == null)
            {
                var timer = new Timer { Interval = 10 };
                timer.Tick += (v, a) => _machine.IntSource.IsEnabled = true;
                timer.Start();
                interruptTimer = timer;
            }

            if (watchTimer == null)
            {
                watchTimer = new Timer { Interval = 500 };
                watchTimer.Tick += (s, a) => {
                    var stats = new MachineStats()
                    {
                        InstructionsPerSecond = (int)(_machine.InstructionCount / (watchTimer.Interval / 1000.0)),
                        FramesPerSecond = graphicsPainter.CurrentFps
                    };
                    _machine.InstructionCount = 0;

                    _view.UpdateStats(stats);
                };
                watchTimer.Start();
            }

            if (debugTimer == null)
            {
                debugTimer = new Timer { Interval = 500 };
                debugTimer.Tick += (s, a) => {
                    _view.UpdateMCStart(StartMcProgram);
                    _view.UpdateMcEnd(EndMCProgram);
                };
                debugTimer.Start();
            }
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

        internal void StartDebug()
        {
            var frm = new frmDebug(_machine.Cpu, _machine.Memory);
            frm.Show();

            var frm2 = new frmMemoryView(_machine.Cpu);
            frm2.Show();
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

        public void KeyDown(int keyValue, Keys keyData)
        {
            _machine.Keyboard.SetKeyState(new Keyboard.KeyState(keyValue, keyData));

            //trigger interrupt early to handle keys fast
            _machine.IntSource.IsEnabled = true;
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

        public AddressRange LoadFile(string fileName)
        {
            var addressRange = fileHandler.LoadFile(fileName);
            StartMcProgram = addressRange.Start;
            EndMCProgram = addressRange.End;
            return addressRange;
        }

        public void SaveDiskImage(string fileName)
        {
            _machine.Drive.SaveDiskImage(fileName);
        }

        public void LoadDiskImage(string fileName)
        {
            _machine.Drive.LoadDiskImage(fileName);
        }

        internal void ToggleUseFixedScaling()
        {
            if (graphicsPainter != null)
            {
                graphicsPainter.UseFixedScale = !graphicsPainter.UseFixedScale;
                graphicsPainter.FixedScale = 2;
            }
        }

        internal void ToggleDisplaySmothing()
        {
            if (graphicsPainter != null)
                graphicsPainter.UseSmoothing = !graphicsPainter.UseSmoothing;
        }

        internal void ToggleGrahpicsMode()
        {
            _machine.OutputLatch.Value ^= (byte)VzConstants.OutputLatchBits.GraphicsMode; //toggle graphics mode
        }

        internal void ToggleColour()
        {
            if (graphicsPainter != null)
                graphicsPainter.GrayScale = !graphicsPainter.GrayScale;
        }

        internal void DevelopmentShowFont()
        {
            var imageForm = new Form();
            imageForm.Text = "Font Tile Viewer";
            imageForm.BackgroundImage = ((TextModeRenderer)graphicsPainter.TextModeRenderer)._FontBitmap.Bitmap;
            imageForm.BackgroundImageLayout = ImageLayout.None;
            imageForm.Show();
        }
    }
}
