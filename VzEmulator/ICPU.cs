using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Konamiman.Z80dotNet;

namespace VzEmulator
{
    public interface ICpu
    {
        IMemoryAccessor Memory { get; set; }
        IInterruptEnableFlag InterruptEnableFlag { get; set; }
        void Reset();
        bool IsHalted { get; }
        IRegisters Registers { get; }
        ProcessorState State { get; } //todo use local enum

        event EventHandler<MemoryAccessEventArgs> MemoryAccess; //todo type
        event EventHandler<AfterInstructionExecutionEventArgs> AfterInstructionExecution; //todo type

        void ExecuteCall(ushort Address);

        void SaveRegistersToMemory(ushort StartAddress);
        void LoadRegistersFromMemory(ushort StartAddress);
        void Continue();
    }

    public interface IRegisters
    {
        ushort PC { get; set; }
        byte A { get; set; }
        byte F { get; set; }
        short AF { get; set; }
        short BC { get; set; }
        short DE { get; set; }
        short HL { get; set; }
        short IX { get; set; }
        short IY { get; set; }
        short SP { get; set; }
    }
}
