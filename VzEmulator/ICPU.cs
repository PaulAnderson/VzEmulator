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
        CpuState State { get; } //todo use local enum

        event EventHandler<BusEventArgs> MemoryAccess; 
        event EventHandler<BusEventArgs> PortAccess; //todo type

        event EventHandler<InstructionEventArgs> AfterInstructionExecution;

        void ExecuteCall(ushort Address);

        void SaveRegistersToMemory(ushort StartAddress);
        void LoadRegistersFromMemory(ushort StartAddress);
        void Continue();
        void Pause();
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

    public enum CpuState
    {
        Stopped = 0,
        Paused = 1,
        Running = 2,
        ExecutingOneInstruction = 3
    }

    public class BusEventArgs
    {
        public bool IsRead { get; }
        public bool IsMemoryAccess { get; }
        public ushort Address { get; }
        public byte Value { get; set; }
        public bool IsComplete { get; set; }

        public BusEventArgs(bool isRead, bool isMemoryAccess, ushort address, byte value)
        {
            this.IsRead = isRead;
            this.IsMemoryAccess = isMemoryAccess;
            this.Address = address;
            this.Value = value;
        }
    }
    public class InstructionEventArgs
    {
        public ushort Address { get; }
        public byte[] OpCode { get; }
        public bool StopWhenComplete { get; set; }

        public InstructionEventArgs(ushort address, byte[] opCode)
        {
            this.Address = address;
            this.OpCode = opCode;
        }
    }

}
