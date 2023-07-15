using System;
using System.Threading.Tasks;
using VzEmulator.Debug;
using VzEmulator.Peripherals;

namespace VzEmulator
{
    public class Machine
    {
        public ICpu Cpu { get; } = new Z80dotNetCpuAdapter();

        internal MemUtils Memory { get; private set; }
        internal VideoMemory VideoMemory { get; set; }
        internal Keyboard Keyboard;
        internal AudioIn AudioInp;
        internal InputLatch InpLatch;
        internal IInterruptEnableFlag IntSource;

        private bool cpuIsSetup = false;
        private bool resetting;
        private InstructionTracer tracer;
        private readonly PeripheralRouter router = new PeripheralRouter();
        private readonly Drive drive = new Drive();
        
        private readonly Rom rom = new Rom();
        private IMemoryAccessor memory;

        private readonly Printer printer = new Printer();
        internal IPrinterOutput PrinterOutput => (IPrinterOutput)printer;

        private AudioOut sound;
        internal IAudioOutput SoundOutput => (IAudioOutput)sound;
        internal IAudioStreamIn SoundInput => (IAudioStreamIn)AudioInp;
        public bool SoundEnabled { set {
                sound.SoundEnabled = value;
            } }
        public bool SoundTestTone { set
            {
                sound.TestTone = value;
            } }

        private readonly MemoryLatch _OutputLatch = new MemoryLatch();
        public MemoryLatch OutputLatch => _OutputLatch;
        public PortLatch AuExtendedGraphicsLatch 
            = new PortLatch(VzConstants.AuExtendedGraphicsLatchPortStart, VzConstants.AuExtendedGraphicsLatchPortEnd, VzConstants.AuExtendedGraphicsLatchDefault)
            { Value = VzConstants.AuExtendedGraphicsLatchDefault };
        public PortLatch DeExtendedGraphicsLatch =
            new PortLatch(VzConstants.DeExtendedGraphicsLatchPortStart, VzConstants.DeExtendedGraphicsLatchPortEnd)
            { LinkedLatchCopyMask = VzConstants.VideoMemoryBankSwitchMask };
  
        public double InstructionCount { get; set; }
        public double ClockCycleCount { get; set; }
        
        public Drive Drive => drive;
        public bool TraceEnabled { get; set; }
        public bool ClockSynced
        {
            get => ((IClockSynced)sound).ClockSyncEnabled;
            set { ((IClockSynced)sound).ClockSyncEnabled = value; } 
        }

        public decimal ClockSpeed
        {
            get =>
                ((IClockSynced)sound).ClockFrequency = ClockSpeed;
            set
            {
                ((IClockSynced)sound).ClockFrequency = value;
            }
        }

        public Machine()
        {
            SetupDevices();
        }

        private void SetupDevices()
        {
            IntSource = Cpu.InterruptEnableFlag;
            Keyboard = new Keyboard(IntSource);
            AudioInp = new AudioIn(Cpu);
            InpLatch = new InputLatch(Keyboard, AudioInp, IntSource); 
            memory = Cpu.Memory;
            VideoMemory = new VideoMemory(memory, AuExtendedGraphicsLatch);
            DeExtendedGraphicsLatch.LinkedLatch = AuExtendedGraphicsLatch; //De Latch stores bits 0,1 value in Au latch
            router.Add(InpLatch).Add(_OutputLatch).Add(rom).Add(VideoMemory).Add(drive).Add(printer).Add(AuExtendedGraphicsLatch).Add(DeExtendedGraphicsLatch);
            sound = new AudioOut(_OutputLatch,AudioInp);
            Cpu.ClockSync = sound;

            ((IClockSynced)sound).ClockSyncEnabled = true;
            ((IClockSynced)sound).ClockFrequency = VzConstants.ClockFrequencyMhz;
        }

        internal void StartCpuTask()
        {
            if (Cpu != null)
                Task.Run(() => Cpu.Continue());
        }

        public void SaveRegistersToMemory(ICpu z80)
        {
            ushort nextAddress = Memory.SaveRegistersToMemory(z80.Registers, 0);
            z80.Memory[nextAddress] = OutputLatch.Value;
            z80.Memory[nextAddress + 1] = AuExtendedGraphicsLatch.Value;

        }

        public void LoadRegistersFromMemory(ICpu z80)
        {
            ushort nextAddress = Memory.LoadRegistersFromMemory(z80.Registers, 0);
            OutputLatch.Value = z80.Memory[nextAddress];
            AuExtendedGraphicsLatch.Value = z80.Memory[nextAddress + 1];
        }

        public void SetAfterInstructionExecutionCallback(Action action)
        {
            _afterInstructionExecutionCallback = action;
        }

        private Action _afterInstructionExecutionCallback;
        private bool _stopAfterNext;

        private void Z80OnAfterInstructionExecution(object sender, InstructionEventArgs args)
        {
            
            InstructionCount += 1;
            ClockCycleCount += args.TStates;

            InpLatch.ProcessClockCycles(args.TStates);
            

            if (resetting)
            {
                Cpu.Reset();
                router.Reset();
                resetting = false;
            }

            if (args==null || args.OpCode==null)
                return; 

            //reset INT on EI (enable interrupts)
            //if (memory[z80.Registers.PC] == 0xFB)
            if (args.OpCode[0] == 0xFB)
            {
#if DEBUG
                Console.WriteLine($"EI at {args.Address:X4} : 0xFB");
#endif

                IntSource.IsEnabled = false;
            }
#if DEBUG

            if (args.OpCode[0] == 0xF3)
            {
                Console.WriteLine($"DI at {args.Address:X4} : 0xF3");
            }
            if (args.OpCode[0]==0xED && args.OpCode[1]==0x46)
            {
                Console.WriteLine($"IM0 at {args.Address:X4} : 0xF3");
            }
            if (args.OpCode[0] == 0xED && args.OpCode[1] == 0x56)
            {
                Console.WriteLine($"IM1 at {args.Address:X4} : 0xF3");
            }
            if (args.OpCode[0] == 0xED && args.OpCode[1] == 0x5E)
            {
                Console.WriteLine($"IM2 at {args.Address:X4} : 0xF3");
#endif
            }

            if (_afterInstructionExecutionCallback != null)
            {
                _afterInstructionExecutionCallback.Invoke();
                _afterInstructionExecutionCallback = null;
            }

            if (_stopAfterNext)
            {
                _stopAfterNext = false;
                args.StopWhenComplete = true;
            }

            if (TraceEnabled)
                tracer.TraceNextInstruction();
             
        }

        private void CpuOnBeforeInstructionExecution(object sender, InstructionEventArgs e)
        {
        }

        private void OnCpuBusAccess(object sender, BusEventArgs args)
        {
            if (args.IsMemoryAccess)
            {
                if (args.IsRead)
                {
#if DEBUG_KEYBOARD
                    if (args.Address == 26624)
                    {
                        Console.Write("KEYB:");
                        tracer.TraceNextInstruction();
                    }
#endif 
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
            }
            else
            {
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

        internal void WaitForCpuStop()
        {
            while (Cpu.State == CpuState.Running)
                System.Threading.Thread.Sleep(0);
        }

        internal void JumpToUsrExec()
        {
            var addr = Memory.GetWordAtAddress(VzConstants.UserExecWordPtr);
            Cpu.ExecuteCall(addr);
        }

        internal void Pause()
        {
            Cpu.Pause();
        }


        public void Setup(byte[] program)
        {
            if (!cpuIsSetup)
            {
                Memory = new MemUtils(memory);
                tracer = new InstructionTracer(Cpu);

                memory.SetContents(0, program);

                //set events
                Cpu.AfterInstructionExecution += Z80OnAfterInstructionExecution;
                Cpu.BeforeInstructionExecution += CpuOnBeforeInstructionExecution;

                Cpu.MemoryAccess += OnCpuBusAccess;
                Cpu.PortAccess += OnCpuBusAccess;

                Cpu.Reset();
                StartCpuTask();

                cpuIsSetup = true;

            }
            else
            {
                memory.SetContents(0, program);

                if (Cpu.IsHalted)
                    Cpu.Reset();
                else
                    resetting = true;
            }
        }

        public void StopCpuAfterNextInstruction()
        {
            _stopAfterNext = true;
        }

        internal void AttachDriveWatcher(IDriveWatcher watcher)
        {
            drive.DriveStatusChangeEvent += (object sender, DriveStatusChange e) => watcher.NotifyDriveStatusChange(e);
        }

        internal void SetClockSync(bool clockSynced)
        {
            throw new NotImplementedException();
        }
    }
}
