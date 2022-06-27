using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VzEmulator.Peripherals
{
    public class PortLatch : IPeripheral , ILatchValue
    {
        public Tuple<ushort, ushort> PortRange => _portRange;

        public Tuple<ushort, ushort> MemoryRange => null;

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private Tuple<ushort, ushort> _portRange;
        
        public byte Value { get; set; }

        public PortLatch(Tuple<ushort, ushort> PortRange)
        {
            _portRange = PortRange;
        }

        public PortLatch(ushort portAddress)
        {
            _portRange = new Tuple<ushort, ushort>(portAddress, portAddress);
        }

        public byte? HandleMemoryRead(ushort address)
        {
            return null;
        }

        public bool HandleMemoryWrite(ushort address, byte value)
        {
            return false;
        }

        public byte? HandlePortRead(ushort address)
        {
            return null;
        }

        public void HandlePortWrite(ushort address, byte value)
        {
            if (_portRange.Item1 <= address & address <= _portRange.Item2)
                this.Value = value;
        }
    }
}
