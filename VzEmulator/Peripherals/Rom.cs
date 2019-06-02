using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VzEmulator.Peripherals
{
    class Rom : IPeripheral
    {
        public Tuple<ushort, ushort> PortRange => null;
        public Tuple<ushort, ushort> MemoryRange { get; } = new Tuple<ushort, ushort>(0, VzConstants.TopOfRom);

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //Value returned from elsewhere
        public byte? HandleMemoryRead(ushort address) => null;

        //Block write if in memory range
        public bool HandleMemoryWrite(ushort address, byte value)
        {
            return (MemoryRange.Item1 <= address & address <= MemoryRange.Item2);
        }

        public byte? HandlePortRead(ushort address)
        {
            return null;
        }

        public void HandlePortWrite(ushort address, byte value)
        {
            //do nothing
        }
    }
}
