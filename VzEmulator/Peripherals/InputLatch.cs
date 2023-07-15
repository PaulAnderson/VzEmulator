using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VzEmulator.Peripherals
{
    public class InputLatch : IPeripheral, ILatchValue, IClockSynced
    {
        public IPeripheral keyboard;
        public IAudioInput audioInput;
        public IInterruptEnableFlag intSource { get; set; }
        public InputLatch(Keyboard keyboard,IAudioInput audioInput, IInterruptEnableFlag interruptSource)
        {
            this.keyboard = keyboard;
            this.audioInput = audioInput;
            this.intSource = interruptSource;
        }

        public Tuple<ushort, ushort> PortRange => null;

        public Tuple<ushort, ushort> MemoryRange => new Tuple<ushort, ushort>(VzConstants.OutputLatchAndKbStart , VzConstants.OutputLatchAndKbEnd);

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte Value { get; set; }
        public decimal ClockFrequency { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool ClockSyncEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        Random random = new Random();
        public byte? HandleMemoryRead(ushort address)
        {
            const byte keyMask = 0b00111111;
            const byte cassetteMask = 0b01000000;
            var keyvalue = keyboard.HandleMemoryRead(address) & keyMask;
            var cassetteValue = audioInput.HandleMemoryRead(address) & cassetteMask;
            //cassetteValue ^= cassetteMask; //invert cassette value
            Value = (byte)(keyvalue | cassetteValue  );
            if (GetRefreshBit()) 
                Value |= 0b10000000;
            else
                Value &= 0b01111111;
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

        int clockCyclesInFrame = 0;
        int CurrentRenderLine = 0; //this is only for setting the input latch value. actual video render is handled elsewhere
        public void ProcessClockCycles(int periodLengthInCycles)
        {
            //Calculate screen refresh
            //From VZ-300 tech manual:
            /*
            25 lines of bottom border (from VDP)
            25 lines of bottom border (from U18, etc.)
            1 line of bottom border (from VDP)
            6 lines of vertical retrace (from VDP)
            13 lines of blanking (from U18, etc.)
            12 lines of top border (from U18, etc.)
            38 lines of top border (from VDP)
            192 lines of active display (from VDP)
            */
            //312 lines per frame at 50fps:
            // 15600 lines per second
            // 3540000 clock cycles per second / 15600 = 226 clock cycles per line

            clockCyclesInFrame += periodLengthInCycles;
            int previousRenderLine = CurrentRenderLine;
            CurrentRenderLine = clockCyclesInFrame / 226;

            if (CurrentRenderLine >= 312)
            {
                clockCyclesInFrame = 0;
                CurrentRenderLine = 0;
            }
            if (CurrentRenderLine == 306)
            {
               intSource.IsEnabled = true;
            }
        }
        public bool GetRefreshBit()
        {
            return CurrentRenderLine < 254;
        }
    }
}
