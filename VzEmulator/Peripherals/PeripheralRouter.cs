using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    public class PeripheralRouter
    {
        private List<IPeripheral> AttachedDevices;
        private MemoryLatch OutputLatch;

        public PeripheralRouter()
        {
            AttachedDevices = new List<IPeripheral>();
        }
        public PeripheralRouter Add(IPeripheral peripheral)
        {
            AttachedDevices.Add(peripheral);
            if (peripheral is MemoryLatch && peripheral.MemoryRange.Item1 == VzConstants.OutputLatchAndKbStart)
            {
                OutputLatch = (MemoryLatch)peripheral;
            }
            return this;
        }

        public byte? HandlePortRead(ushort address)
        {
            foreach (var device in AttachedDevices)
            {
                if (device.PortRange!=null && address >= device.PortRange.Item1 && address <= device.PortRange.Item2 )
                {
                    return device.HandlePortRead(address);
                }
            }
            return null;
        }
        public void HandlePortWrite(ushort address, byte value)
        {
            foreach (var device in AttachedDevices)
            {
                if (device.PortRange != null && address >= device.PortRange.Item1 && address <= device.PortRange.Item2)
                {
                    device.HandlePortWrite(address,value);
                }
            }
        }
        public byte? HandleMemoryRead(ushort address)
        {
            if (address == VzConstants.OutputLatchAndKbStart)
            {
                return OutputLatch.HandleMemoryRead(address);
            }
            foreach (var device in AttachedDevices)
            {
                if (device.MemoryRange != null && address >= device.MemoryRange.Item1 && address <= device.MemoryRange.Item2)
                {
                    return device.HandleMemoryRead(address);
                }
            }
            return null;
        }
        public bool HandleMemoryWrite(ushort address, byte value)
        {
            if (address == VzConstants.OutputLatchAndKbStart)
            {
                OutputLatch.HandleMemoryWrite(address, value);
                return true;
            }

            bool result = false;
            foreach (var device in AttachedDevices)
            {
                if (device.MemoryRange != null && address >= device.MemoryRange.Item1 && address <= device.MemoryRange.Item2)
                {
                    result |= device.HandleMemoryWrite(address, value);
                }
            }
            return result;
        }

        internal void Reset()
        {
            AttachedDevices.ForEach(x => x.Reset());
        }
    }
}
