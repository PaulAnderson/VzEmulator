using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VzEmulate2;

namespace VzEmulator.Peripherals
{
    class Rom : IPeripheral
    {
        public Tuple<ushort, ushort> PortRange => null;

        public Tuple<ushort, ushort> MemoryRange => new Tuple<ushort, ushort>(0, VzConstants.TopOfRom);

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte? HandleMemoryRead(ushort address)
        {
            return null;
        }

        public bool HandleMemoryWrite(ushort address, byte value)
        {
            return (MemoryRange.Item1 >= address & address >= MemoryRange.Item2);
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
