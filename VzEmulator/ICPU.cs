using Konamiman.Z80dotNet;
using System;

namespace VzEmulator
{
    public interface ICpu
    {
        IMemoryAccessor Memory { get; set; }
        IInterruptEnableFlag InterruptEnableFlag { get; set; }
        void Reset();
        bool IsHalted { get; }
        IRegisters Registers { get; }
        CpuState State { get; }
        IClockSynced ClockSync { get; set; }
        decimal ClockSpeedMhz { get; set; }

        event EventHandler<BusEventArgs> MemoryAccess;
        event EventHandler<BusEventArgs> PortAccess;
        event EventHandler<InstructionEventArgs> AfterInstructionExecution;
        event EventHandler<InstructionEventArgs> BeforeInstructionExecution;

        void ExecuteCall(ushort address);
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
        short AltAF { get; set; }
        short AltBC { get; set; }
        short AltDE { get; set; }
        short AltHL { get; set; }
        short IX { get; set; }
        short IY { get; set; }
        short SP { get; set; }

        bool IFF1 { get; set; }
        bool IFF2 { get; set; }
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
        public int TStates { get; set; }
        public bool StopWhenComplete { get; set; }

        public InstructionEventArgs(ushort address, byte[] opCode)
        {
            this.Address = address;
            this.OpCode = opCode;
            this.TStates = 0;
        }
        public InstructionEventArgs(ushort address, byte[] opCode,int tStates)
        {
            this.Address = address;
            this.OpCode = opCode;
            this.TStates = tStates;
        }
    }

}
