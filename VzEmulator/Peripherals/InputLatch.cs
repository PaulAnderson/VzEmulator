using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VzEmulator.Peripherals
{
    public class InputLatch : IPeripheral, ILatchValue
    {
        public IPeripheral keyboard;
        public IAudioInput audioInput;

        public InputLatch(Keyboard keyboard,IAudioInput audioInput)
        {
            this.keyboard = keyboard;
            this.audioInput = audioInput;
        }

        public Tuple<ushort, ushort> PortRange => null;

        public Tuple<ushort, ushort> MemoryRange => new Tuple<ushort, ushort>(VzConstants.OutputLatchAndKbStart , VzConstants.OutputLatchAndKbEnd);

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte Value { get; set; }

        public byte? HandleMemoryRead(ushort address)
        {
            const byte keyMask = 0b00111111;
            const byte cassetteMask = 0b11000000;
            var keyvalue = keyboard.HandleMemoryRead(address) & keyMask;
            var cassetteValue = audioInput.HandleMemoryRead(address) & cassetteMask;
            Value = (byte)(keyvalue | cassetteValue);
            return Value;
        }

        public bool HandleMemoryWrite(ushort address, byte value)
        {
            //Do nothing. Can't write to input latch
            return false;
        }

        public byte? HandlePortRead(ushort address)
        {
            return null;
        }

        public void HandlePortWrite(ushort address, byte value)
        {
        }

        public void Reset()
        {
            this.Value = 0;
        }
    }
}
