using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace VzEmulator.Peripherals
{
    class Rom : IPeripheral
    {
        public Tuple<ushort, ushort> PortRange => null;
        public Tuple<ushort, ushort> MemoryRange { get; } = new Tuple<ushort, ushort>(0, VzConstants.TopOfRom);

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        byte csave = 0xf4;
        //Value returned from elsewhere
        public byte? HandleMemoryRead(ushort address)
        {
            return null;
            //ROM patches go here

            //return values representing 'PAUL' in hex for 0x010F onwards
            //if (address >= 0x010F)
            //{
            //    switch (address)
            //    {
            //        case 0x010F:
            //            return 0x50;
            //        case 0x0110:
            //            return 0x41;
            //        case 0x0111:
            //            return 0x55;
            //        case 0x0112:
            //            return 0x4C;
            //        case 0x0113:
            //            return 0x20;
            //        case 0x34AB: //CSAVE Program type;. F0 for basic "T", F1 for machine code, "B",F2 for data "D", F3 for "W" (runs as basic)
            //            if (csave == 0xff) csave = 0;
            //            csave += 1;
            //            return csave;

            //        default:
            //            return null;
            //    }
            //}
            //else
            //{
            //    return null;
            //}
        }
         
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

        public void Reset()
        {
            //do nothing
        }
    }
}
