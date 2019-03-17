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

        public PeripheralRouter()
        {
            AttachedDevices = new List<IPeripheral>();
        }
        public void Add(IPeripheral peripheral)
        {
            AttachedDevices.Add(peripheral);
        }

        public byte? HandlePortRead(ushort address)
        {
            foreach (var device in AttachedDevices)
            {
                if (address >= device.PortRange.Item1 && address <= device.PortRange.Item2 )
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
                if (address >= device.PortRange.Item1 && address <= device.PortRange.Item2)
                {
                    device.HandlePortWrite(address,value);
                }
            }
        }
        public byte? HandleMemoryRead(ushort address)
        {
            foreach (var device in AttachedDevices)
            {
                if (address >= device.MemoryRange.Item1 && address <= device.MemoryRange.Item2)
                {
                    return device.HandleMemoryRead(address);
                }
            }
            return null;
        }
        public void HandleMemoryWrite(ushort address, byte value)
        {
            foreach (var device in AttachedDevices)
            {
                if (address >= device.MemoryRange.Item1 && address <= device.MemoryRange.Item2)
                {
                    device.HandleMemoryWrite(address, value);
                }
            }
        }
    }
}
