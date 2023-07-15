using Konamiman.Z80dotNet;
using System;

namespace VzEmulator
{
    class Z80dotNetCpuAdapter : ICpu
    {
        private Z80Processor _cpu;
        private IMemoryAccessor _memory;

        IMemoryAccessor ICpu.Memory { get => _memory; set => _memory = value; }

        IInterruptEnableFlag ICpu.InterruptEnableFlag { get => intSource; set => throw new NotImplementedException(); }

        Z80DotNetClockSynchronizer z80DotNetClockSynchronizer;

        public IClockSynced ClockSync { get => z80DotNetClockSynchronizer.ClockSyncedObject; set => z80DotNetClockSynchronizer.ClockSyncedObject = value; }

        public bool IsHalted => _cpu.IsHalted;

        private Z80DotNetRegisterAdapter _registers;
        public IRegisters Registers => _registers;

        public CpuState State => (CpuState)(int) _cpu.State;

        public decimal ClockSpeedMhz { get => _cpu.ClockFrequencyInMHz; set  { _cpu.ClockFrequencyInMHz = value; } }

        InterruptSource intSource = new InterruptSource();
        private bool _isStopping;

        public Z80dotNetCpuAdapter()
        {
            _cpu = new Z80Processor();
            _memory = new Z80dotNetMemoryAccessor(_cpu.Memory);

            _registers = new Z80DotNetRegisterAdapter(_cpu);

            _cpu.AutoStopOnDiPlusHalt = false;
            _cpu.AutoStopOnRetWithStackEmpty = false;

            z80DotNetClockSynchronizer =  new Z80DotNetClockSynchronizer();
            _cpu.ClockSynchronizer =  z80DotNetClockSynchronizer;

            _cpu.RegisterInterruptSource(intSource);
            _cpu.InterruptMode = 0;

            _cpu.AfterInstructionExecution += OnCpuAfterInstructionExecution;
            _cpu.BeforeInstructionExecution += CpuOnBeforeInstructionExecution;
            _cpu.MemoryAccess += OnCpuMemoryAccess;

        }

        private void CpuOnBeforeInstructionExecution(object sender, BeforeInstructionExecutionEventArgs args)
        {
            //map
            var instructionEventArgs = new InstructionEventArgs(_cpu.Registers.PC, args.Opcode);
            OnRaiseBeforeInstructionExecution(instructionEventArgs);
        }

        private void OnCpuAfterInstructionExecution(object sender, AfterInstructionExecutionEventArgs args)
        {
            //map
            var instructionEventArgs = new InstructionEventArgs(_cpu.Registers.PC, args.Opcode,args.TotalTStates);

            OnRaiseAfterInstructionExecution(instructionEventArgs);

            //map
            if (instructionEventArgs.StopWhenComplete | _isStopping)
            {
                args.ExecutionStopper.Stop(isPause: true);
                _isStopping = false;
            }
        }

        public event EventHandler<InstructionEventArgs> AfterInstructionExecution;
        public event EventHandler<InstructionEventArgs> BeforeInstructionExecution;


        protected virtual void OnRaiseAfterInstructionExecution(InstructionEventArgs e)
        {
            AfterInstructionExecution?.Invoke(this, e);
        }

        protected virtual void OnRaiseBeforeInstructionExecution(InstructionEventArgs e)
        {
            BeforeInstructionExecution?.Invoke(this, e);
        }

        private void OnCpuMemoryAccess(object sender, MemoryAccessEventArgs args)
        {
            var busEventArgs = MapBusEventArgs(args);

            //raise event
            if (busEventArgs != null)
            {
                if (busEventArgs.IsMemoryAccess)
                {
                    OnRaiseMemoryAccess(busEventArgs);
                }
                else
                {
                    OnRaisePortAccess(busEventArgs);
                }

                //map result
                args.CancelMemoryAccess = busEventArgs.IsComplete;
                args.Value = busEventArgs.Value;
            }
        }
        private BusEventArgs MapBusEventArgs(MemoryAccessEventArgs args)
        {
            bool isRead;
            bool isMemory;

            switch (args.EventType)
            {
                case MemoryAccessEventType.AfterPortWrite:
                    isRead = false;
                    isMemory = false;
                    break;
                case MemoryAccessEventType.BeforePortRead:
                    isRead = true;
                    isMemory = false;
                    break;
                case MemoryAccessEventType.BeforeMemoryWrite:
                    isRead = false;
                    isMemory = true;
                    break;
                case MemoryAccessEventType.BeforeMemoryRead:
                    isRead = true;
                    isMemory = true;
                    break;
                default:
                    return null;
            }

            return new BusEventArgs(isRead, isMemory, args.Address, args.Value);
        }
        public event EventHandler<BusEventArgs> MemoryAccess;

        protected virtual void OnRaiseMemoryAccess(BusEventArgs e)
        {
            MemoryAccess?.Invoke(this, e);
        }

        public event EventHandler<BusEventArgs> PortAccess;

        protected virtual void OnRaisePortAccess(BusEventArgs e)
        {
            PortAccess?.Invoke(this, e);
        }

        public void Reset()
        {
            _cpu.Reset();
        }

        public void ExecuteCall(ushort Address)
        {
            _cpu.ExecuteCall(Address);
        }

        public void Continue()
        {
            _isStopping = false;
            _cpu.Continue();
        }

        public void Pause()
        {

            _isStopping = true;
        }

        class Z80DotNetRegisterAdapter : IRegisters
        {
            Z80Processor _cpu;
            IZ80Registers _registers;

            public Z80DotNetRegisterAdapter(Z80Processor cpu)
            {
                _cpu = cpu;
                _registers = _cpu.Registers;
            }

            public byte A { get => _registers.A; set => _registers.A = value; }
            public byte F { get => _registers.F; set => _registers.F = value; }
            public short AF { get => _registers.AF; set => _registers.AF = value; }
            public short BC { get => _registers.BC; set => _registers.BC = value; }
            public short DE { get => _registers.DE; set => _registers.DE = value; }
            public short HL { get => _registers.HL; set => _registers.HL = value; }
            public short AltAF { get => _registers.Alternate.AF; set => _registers.Alternate.AF = value; }
            public short AltBC { get => _registers.Alternate.BC; set => _registers.Alternate.BC = value; }
            public short AltDE { get => _registers.Alternate.DE; set => _registers.Alternate.DE = value; }
            public short AltHL { get => _registers.Alternate.HL; set => _registers.Alternate.HL = value; }
            public short IX { get => _registers.IX; set => _registers.IX = value; }
            public short IY { get => _registers.IY; set => _registers.IY = value; }
            public short SP { get => _registers.SP; set => _registers.SP = value; }
            public ushort PC { get => _registers.PC; set => _registers.PC = value; }

        }
    }
}
