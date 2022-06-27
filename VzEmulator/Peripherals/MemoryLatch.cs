using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VzEmulator.Peripherals
{
    public class MemoryLatch : IPeripheral, ILatchValue
    {
        public Tuple<ushort, ushort> PortRange => null;

        public Tuple<ushort, ushort> MemoryRange => new Tuple<ushort, ushort>(VzConstants.OutputLatchAndKbStart , VzConstants.OutputLatchAndKbEnd);

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte Value { get; set; }

        public byte? HandleMemoryRead(ushort address)
        {
            return null;
        }

        public bool HandleMemoryWrite(ushort address, byte value)
        {
            if (MemoryRange.Item1 <= address & address <= MemoryRange.Item2)
                this.Value = value;
            return false;
        }

        public byte? HandlePortRead(ushort address)
        {
            return null;
        }

        public void HandlePortWrite(ushort address, byte value)
        {
        }
    }
}
