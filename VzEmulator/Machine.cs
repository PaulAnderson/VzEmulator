using System;
using System.Threading.Tasks;
using VzEmulator.Debug;
using VzEmulator.Peripherals;

namespace VzEmulator
{
    public class Machine
    {
        private readonly ICpu cpu = new Z80dotNetCpuAdapter();
        public ICpu Cpu { get => cpu; }

        private MemUtils mem;
        internal MemUtils Memory { get => mem; }
        internal VideoMemory VideoMemory { get; set; }
        internal Keyboard Keyboard;
        internal IInterruptEnableFlag IntSource;

        private bool cpuIsSetup = false;
        private bool resetting;
        private InstructionTracer tracer;
        private readonly PeripheralRouter router = new PeripheralRouter();
        private readonly Drive drive = new Drive();
        private readonly OutputLatch _OutputLatch = new OutputLatch();
        private readonly Rom rom = new Rom();
        private IMemoryAccessor memory;
        private Sound sound;

        public OutputLatch OutputLatch => _OutputLatch;
        public double InstructionCount { get; set; }
        public Drive Drive => drive;
        public bool TraceEnabled { get; set; }

        public Machine()
        {
            SetupDevices();
        }

        private void SetupDevices()
        {
            IntSource = cpu.InterruptEnableFlag;
            Keyboard = new Keyboard(IntSource);
            memory = cpu.Memory;
            VideoMemory = new VideoMemory(memory);
            router.Add(drive).Add(Keyboard).Add(_OutputLatch).Add(rom).Add(VideoMemory);
            sound = new Sound(_OutputLatch, cpu);
        }

        internal void StartCpuTask()
        {
            if (cpu != null)
                Task.Run(() => cpu.Continue());
        }

        public void SaveRegistersToMemory(ICpu z80)
        {
            ushort nextAddress = mem.SaveRegistersToMemory(z80.Registers, 0);
            z80.Memory[nextAddress] = OutputLatch.Value;
        }

        public void LoadRegistersFromMemory(ICpu z80)
        {
            ushort nextAddress = mem.LoadRegistersFromMemory(z80.Registers, 0);
            OutputLatch.Value = z80.Memory[nextAddress];
        }

        public void SetAfterInstructionExecutionCallback(Action action)
        {
            _afterInstructionExecutionCallback = action;
        }

        private Action _afterInstructionExecutionCallback;
        private bool _stopAfterNext;

        private void Z80OnAfterInstructionExecution(object sender, InstructionEventArgs args)
        {
            
            //var z80 = (ICpu)sender;

            InstructionCount += 1;

            if (resetting)
            {
                cpu.Reset();
                resetting = false;
            }

            //reset INT on EI (enable interrupts)
            //if (memory[z80.Registers.PC] == 0xFB)
            if (args.OpCode[0] == 0xFB)
            {
                IntSource.IsEnabled = false;
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
            while (cpu.State == CpuState.Running)
                System.Threading.Thread.Sleep(0);
        }

        internal void JumpToUsrExec()
        {
            var addr = mem.GetWordAtAddress(VzConstants.UserExecWordPtr);
            cpu.ExecuteCall(addr);
        }

        internal void Pause()
        {
            cpu.Pause();
        }


        public void Setup(byte[] program)
        {
            if (!cpuIsSetup)
            {
                mem = new MemUtils(memory);
                tracer = new InstructionTracer(cpu);

                memory.SetContents(0, program);

                //set events
                cpu.AfterInstructionExecution += Z80OnAfterInstructionExecution;
                cpu.BeforeInstructionExecution += CpuOnBeforeInstructionExecution;

                cpu.MemoryAccess += OnCpuBusAccess;
                cpu.PortAccess += OnCpuBusAccess;

                cpu.Reset();
                StartCpuTask();

                cpuIsSetup = true;

            }
            else
            {
                memory.SetContents(0, program);

                if (cpu.IsHalted)
                    cpu.Reset();
                else
                    resetting = true;
            }
        }

        public void StopCpuAfterNextInstruction()
        {
            _stopAfterNext = true;
        }
    }
}
