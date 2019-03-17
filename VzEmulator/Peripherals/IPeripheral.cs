using System;

namespace VzEmulator.Peripherals
{
    public interface IPeripheral
    {
        Tuple<ushort, ushort> PortRange { get; }
        Tuple<ushort, ushort> MemoryRange { get; }
        bool DebugEnabled { get; set; }
        byte? HandlePortRead(ushort address);
        void HandlePortWrite(ushort address,byte value);
        byte? HandleMemoryRead(ushort address);
        void HandleMemoryWrite(ushort address, byte value);

    }
}
