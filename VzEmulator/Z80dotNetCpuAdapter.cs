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
            //todo map
            OnRaiseAfterInstructionExecution(args);
            //todo map
        }

        public event EventHandler<AfterInstructionExecutionEventArgs> AfterInstructionExecution;
        
        protected virtual void OnRaiseAfterInstructionExecution(AfterInstructionExecutionEventArgs e)
        {
            AfterInstructionExecution?.Invoke(this, e);
        }

        private void OnCpuMemoryAccess(object sender, MemoryAccessEventArgs args)
        {
            //todo map
            OnRaiseMemoryAccess(args);
            //todo map
        }

        public event EventHandler<MemoryAccessEventArgs> MemoryAccess;

        protected virtual void OnRaiseMemoryAccess(MemoryAccessEventArgs e)
        {
            MemoryAccess?.Invoke(this, e);
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
            _cpu.Continue();
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
