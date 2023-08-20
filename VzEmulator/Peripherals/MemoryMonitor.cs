using System;

namespace VzEmulator.Peripherals
{
    public class MemoryMonitor : IPeripheral, IMemoryMonitor
    {
        Tuple<ushort, ushort> IPeripheral.PortRange => null;

        public Tuple<ushort, ushort> MemoryRange => new Tuple<ushort, ushort>(VzConstants.VideoRamStart, VzConstants.VideoRamEnd);

        bool IPeripheral.DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<AddressChangedEventArgs> MemoryChanged;

        byte? IPeripheral.HandleMemoryRead(ushort address)
        {
            return null;
        }

        bool IPeripheral.HandleMemoryWrite(ushort address, byte value)
        {
            if ( MemoryRange.Item1 <= address & address <= MemoryRange.Item2)
            {
                OnMemoryChanged( address, value);        
            }
            return false;
        }

        private void OnMemoryChanged(ushort address, byte value)
        {
            MemoryChanged?.Invoke(this, new AddressChangedEventArgs(address,0x00, value));
        }

        byte? IPeripheral.HandlePortRead(ushort address)
        {
            return null;
        }

        void IPeripheral.HandlePortWrite(ushort address, byte value)
        {
        }

        void IPeripheral.Reset()
        {
        }
    }
}
