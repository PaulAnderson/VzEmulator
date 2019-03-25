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

        public ProcessorState State => _cpu.State;//todo

        InterruptSource intSource = new InterruptSource();

        public event EventHandler<bool> AfterInstructionExecution;
        public event EventHandler<MemoryAccessEventArgs> MemoryAccess;
                                                                                                                            
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

        }

        event EventHandler<AfterInstructionExecutionEventArgs> ICpu.AfterInstructionExecution
        {
            add
            {
                _cpu.AfterInstructionExecution += value; //todo
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<MemoryAccessEventArgs> ICpu.MemoryAccess
        {
            add
            {
                _cpu.MemoryAccess += value; //todo
            }

            remove
            {
                throw new NotImplementedException();
            }
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
