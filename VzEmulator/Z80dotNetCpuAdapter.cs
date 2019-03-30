using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator
{
    class Z80dotNetCpuAdapter : ICpu
    {
        private Z80Processor _cpu;
        private IMemoryAccessor _memory;

        IMemoryAccessor ICpu.Memory { get => _memory; set => _memory = value; }

        IInterruptEnableFlag ICpu.InterruptEnableFlag { get => intSource; set => throw new NotImplementedException(); }

        public bool IsHalted => _cpu.IsHalted;

        private Z80DotNetRegisterAdapter _registers;
        public IRegisters Registers => _registers;

        public CpuState State => (CpuState)(int) _cpu.State;

        InterruptSource intSource = new InterruptSource();
        private bool _isStopping;

        public Z80dotNetCpuAdapter()
        {
            _cpu = new Z80Processor();
            _memory = new Z80dotNetMemoryAccessor(_cpu.Memory);

            _registers = new Z80DotNetRegisterAdapter(_cpu);

            _cpu.AutoStopOnDiPlusHalt = false;
            _cpu.AutoStopOnRetWithStackEmpty = false;

            _cpu.ClockSynchronizer = null;
            _cpu.RegisterInterruptSource(intSource);

            _cpu.RegisterInterruptSource(intSource);

            _cpu.AfterInstructionExecution += OnCpuAfterInstructionExecution;
            _cpu.MemoryAccess += OnCpuMemoryAccess;


        }

        private void OnCpuAfterInstructionExecution(object sender, AfterInstructionExecutionEventArgs args)
        {
            //map
            var instructionEventArgs = new InstructionEventArgs(_cpu.Registers.PC, args.Opcode);

            OnRaiseAfterInstructionExecution(instructionEventArgs);

            //map
            if (instructionEventArgs.StopWhenComplete | _isStopping)
            {
                args.ExecutionStopper.Stop(isPause: true);
                _isStopping = false;
            }
        }

        public event EventHandler<InstructionEventArgs> AfterInstructionExecution;
        
        protected virtual void OnRaiseAfterInstructionExecution(InstructionEventArgs e)
        {
            AfterInstructionExecution?.Invoke(this, e);
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

        public void SaveRegistersToMemory(ushort StartAddress)
        {
            throw new NotImplementedException();
        }

        public void LoadRegistersFromMemory(ushort StartAddress)
        {
            throw new NotImplementedException();
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
            public short IX { get => _registers.IX; set => _registers.IX = value; }
            public short IY { get => _registers.IY; set => _registers.IY = value; }
            public short SP { get => _registers.SP; set => _registers.SP = value; }
            public ushort PC { get => _registers.PC; set => _registers.PC = value; }

        }
    }
}
