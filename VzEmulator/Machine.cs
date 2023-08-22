using CSCore.Streams;
using System;
using System.Resources;
using System.Threading.Tasks;
using VzEmulator.Debug;
using VzEmulator.Peripherals;
using static System.Windows.Forms.AxHost;

namespace VzEmulator
{
    public class Machine
    {
        public String Tag { get; set; }
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

        private readonly MemoryMonitor ScreenMemoryMonitor = new MemoryMonitor();
        internal IMemoryMonitor ScreenOutput => (IMemoryMonitor)ScreenMemoryMonitor;


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
            InpLatch = new InputLatch(Keyboard, AudioInp, IntSource) { Tag = Tag }; 
            memory = Cpu.Memory;
            VideoMemory = new VideoMemory(memory, AuExtendedGraphicsLatch);
            DeExtendedGraphicsLatch.LinkedLatch = AuExtendedGraphicsLatch; //De Latch stores bits 0,1 value in Au latch
            router.Add(InpLatch).Add(_OutputLatch).Add(rom).Add(VideoMemory).Add(drive).Add(printer).Add(AuExtendedGraphicsLatch).Add(DeExtendedGraphicsLatch).Add(ScreenMemoryMonitor);
            sound = new AudioOut(_OutputLatch,AudioInp);
            Cpu.ClockSync = sound;

            //todo a list of clock synced devices
            ((IClockSynced)sound).ClockSyncEnabled = true;
            ((IClockSynced)sound).ClockFrequency = VzConstants.ClockFrequencyMhz;
            ((IClockSynced)drive).ClockSyncEnabled = false;
            ((IClockSynced)drive).ClockFrequency = VzConstants.ClockFrequencyMhz;
            ((IClockSynced)Keyboard).ClockSyncEnabled = false;
            ((IClockSynced)Keyboard).ClockFrequency = VzConstants.ClockFrequencyMhz;

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
        public byte[] GetRegistersAndMachineState(ICpu z80)
        {
            ushort address = 0;
            ushort stateLength = 32;
            var state = new byte[stateLength];
            SetWordInArray(state, address, stateLength); //First byte of the state is the length of registers etc before memory starts
            address += 2;
            SetSWordInArray(state, address, z80.Registers.AF);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.BC);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.DE);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.HL);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.IX);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.IY);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.SP);
            address += 2;
            SetWordInArray(state, address, z80.Registers.PC);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.AltAF);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.AltBC);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.AltDE);
            address += 2;
            SetSWordInArray(state, address, z80.Registers.AltHL);
            address += 2;
            state[address] = z80.Registers.IFF1 ? (byte)1 : (byte)0;
            state[address] |= z80.Registers.IFF2 ? (byte)2 : (byte)0;
            address += 1;
            state[address] = OutputLatch.Value;
            address += 1;
            state[address] = AuExtendedGraphicsLatch.Value;
            //TODO interrupt mode, interrupt enabled 
            return state;
        }

        //Applies state of stored registers and machine state. Returns length of state excluding memory
        public ushort ApplyRegistersAndMachineState(ICpu z80, byte[] state)
        {
            ushort address = 0;
            ushort statelength = GetWordInArray(state, 0);

            //Check minimum supported state length
            if (statelength < 32)
                throw new Exception("Invalid state length");

            address += 2;
            z80.Registers.AF = GetSWordInArray(state,address);
            address += 2;
            z80.Registers.BC = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.DE = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.HL = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.IX = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.IY = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.SP = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.PC = GetWordInArray(state,address);
            address += 2;
            z80.Registers.AltAF = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.AltBC = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.AltDE = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.AltHL = GetSWordInArray(state, address);
            address += 2;
            z80.Registers.IFF1 = (state[address] & 1) == 1;
            z80.Registers.IFF1 = (state[address] & 2) == 2;
            address += 1;
            OutputLatch.Value = state[address];
            address += 1;
            AuExtendedGraphicsLatch.Value = state[address];

            return statelength;
        }

        public void SetSWordInArray(byte[] array, ushort address, short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            array[address] = bytes[0];
            array[address + 1] = bytes[1];
        }

        public short GetSWordInArray(byte[] array, ushort address)
        {
            byte[] bytes = new byte[2] { array[address], array[address + 1] };
            return BitConverter.ToInt16(bytes, 0);
        }
       
        public void SetWordInArray(byte[] array, ushort address, ushort value)
        {
            array[address] = (byte)(value & 0xFF);
            array[address + 1] = (byte)((value & 0xFF00) >> 8);
        }
        public ushort GetWordInArray(byte[] array, ushort address)
        {
            var result = (ushort)(array[address + 1] << 8 | array[address]);
            return result;
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
        public void SetAfterRetiInstructionExecutionCallback(Action action)
        {
            _afterRetiInstructionExecutionCallback = action;
        }
        public void SetCyclesCallback(Action action, int cycles)
        {
            _afterCyclesCallback = action;
            _afterCyclesCount = ClockCycleCount+cycles;
        }

        private Action _afterInstructionExecutionCallback;
        private Action _afterRetiInstructionExecutionCallback;
        private Action _afterCyclesCallback;
        private double _afterCyclesCount;
        private bool _stopAfterNext;

        private void Z80OnAfterInstructionExecution(object sender, InstructionEventArgs args)
        {
            
            InstructionCount += 1;
            ClockCycleCount += args.TStates;

            //todo a list of clock cycle watchers
            //Clock sync for devices that care about the current clock cycle but are not used to control the cpu speed
            ((IClockSynced)InpLatch).ProcessClockCycles(args.TStates);
            ((IClockSynced)drive).ProcessClockCycles(args.TStates);
            ((IClockSynced)Keyboard).ProcessClockCycles(args.TStates);


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
            if (args.OpCode[0] == 0xFB) //EI
            {
#if DEBUG
                //Console.WriteLine($"EI at {args.Address:X4} : 0xFB");
#endif
                //Clear interupt line, read for the next interrupt.
                IntSource.IsEnabled = false;
            }
            /*
#if DEBUG

            if (args.OpCode[0] == 0xF3)
            {
                Console.WriteLine($"DI at {args.Address:X4} : 0xF3");
            }
            if (args.OpCode[0] == 0xED && args.OpCode[1] == 0x46)
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

            }
#endif
            */
            
            if (_afterInstructionExecutionCallback != null)
            {
                var callback = _afterInstructionExecutionCallback;
                _afterInstructionExecutionCallback = null;
                callback.Invoke();
            }
            if (args.OpCode[0]==0xED && args.OpCode[1]==0x4D)
            {
                if (_afterRetiInstructionExecutionCallback != null)
                {
                    var callback = _afterRetiInstructionExecutionCallback;
                    _afterRetiInstructionExecutionCallback = null;
                    callback.Invoke();
                }
            }
            if (_afterCyclesCallback !=null && ClockCycleCount>_afterCyclesCount)
            {
                var callback = _afterCyclesCallback;
                _afterCyclesCallback = null;
                callback.Invoke();
            }

            if (_stopAfterNext)
            {
                _stopAfterNext = false;
                args.StopWhenComplete = true;
            }

            if (TraceEnabled)
                tracer.TraceNextInstruction();
             
        }

        private void CpuOnBeforeInstructionExecution(object sender, InstructionEventArgs args)
        {
        
        if (args.Address == 0x1D37)//RUN keyword
            {
                Console.WriteLine($"{args.Address:X4}  " +
                       $"A:{Cpu.Registers.AF:X4}," +
                       $"BC:{Cpu.Registers.BC:X4}," +
                       $"DE:{Cpu.Registers.DE:X4}," +
                       $"HL:{Cpu.Registers.HL:X4}," +
                       $"IX:{Cpu.Registers.IX:X4}," +
                       $"IY:{Cpu.Registers.IY:X4}," +
                       $"SP:{Cpu.Registers.SP:X4}," +
                       $"PC:{Cpu.Registers.PC:X4}," +
                       $"Int State:{Cpu.InterruptEnableFlag.IsEnabled:X2}," + 
                        $"IFF1:{Cpu.Registers.IFF1:X2},");

            }
#if DEBUG_TRACE
            if (args.OpCode[0] == 0xC4 ||
                args.OpCode[0] == 0xD4 ||
                args.OpCode[0] == 0xE4 ||
                args.OpCode[0] == 0xF4 ||
                args.OpCode[0] == 0xCC ||
                args.OpCode[0] == 0xDC ||
                args.OpCode[0] == 0xEC ||
                args.OpCode[0] == 0xFC ||
                args.OpCode[0] == 0xCD 
                
                )
            {
                ushort targetAddress = (ushort)((args.OpCode[2] << 8) + args.OpCode[1]);
                if (targetAddress != 0x787d &&
                    targetAddress != 0x3f7b &&
                    targetAddress != 0x2edc &&
                    targetAddress != 0x2efd &&
                    targetAddress != 0x301b &&
                    targetAddress != 0x3430)
                {
                    if (args.OpCode.Length == 3) 
                    Console.WriteLine($"{ClockCycleCount} CALL ({args.OpCode[0]:X2}) at {args.Address:X4} to {args.OpCode[2]:X2}{args.OpCode[1]:X2}");
                    if (args.OpCode.Length == 2)
                        Console.WriteLine($"{ClockCycleCount} CALL ({args.OpCode[0]:X2}) at {args.Address:X4} to {args.OpCode[1]:X2}");
                    if (args.OpCode.Length == 1)
                        Console.WriteLine($"{ClockCycleCount} CALL ({args.OpCode[0]:X2}) at {args.Address:X4} ");
                    if (args.OpCode.Length == 0)
                        Console.WriteLine($"{ClockCycleCount}  ");

                    Console.WriteLine($"{args.Address:X4}  " +
                        $"A:{Cpu.Registers.AF:X4}," +
                        $"BC:{Cpu.Registers.BC:X4}," +
                        $"DE:{Cpu.Registers.DE:X4}," +
                        $"HL:{Cpu.Registers.HL:X4}," +
                        $"IX:{Cpu.Registers.IX:X4}," +
                        $"IY:{Cpu.Registers.IY:X4}," +
                        $"SP:{Cpu.Registers.SP:X4}," +
                        $"PC:{Cpu.Registers.PC:X4},");
                }

                //main loop calls
                /*CALL (CD) at 2EBF to 787D
                CALL (CD) at 2EC2 to 3F7B
                CALL (CD) at 2EC5 to 2EDC
                CALL (CD) at 2EC8 to 2EFD
                CALL (CC) at 2ED1 to 301B
                CALL (CD) at 2ED5 to 3430*/
            } 
            if (args.Address== 0x1D37)
            {
                //write all cpu registers to console
                Console.WriteLine($"{args.Address:X4}  " +
                        $"A:{Cpu.Registers.AF:X4}," +
                        $"BC:{Cpu.Registers.BC:X4}," +
                        $"DE:{Cpu.Registers.DE:X4}," +
                        $"HL:{Cpu.Registers.HL:X4}," +
                        $"IX:{Cpu.Registers.IX:X4}," +
                        $"IY:{Cpu.Registers.IY:X4}," +
                        $"SP:{Cpu.Registers.SP:X4}," +
                        $"PC:{Cpu.Registers.PC:X4},");
            }
#endif
        }

        private void OnCpuBusAccess(object sender, BusEventArgs args)
        {
            if (args.IsMemoryAccess)
            {
                if (args.IsRead)
                {
                    if (args.Address>=0x7aE9 && args.Address < 0x7aF9)
                    {
                          Console.WriteLine($"Read from basic program memory {args.Address:X4}");
                    }
                    if (args.Address == 0x78a4  )
                    {
                        Console.WriteLine($"Read from basic program memory {args.Address:X4}");
                    }
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

        internal void Call(ushort v)
        {
            Cpu.ExecuteCall(v);
        }
    }
}
