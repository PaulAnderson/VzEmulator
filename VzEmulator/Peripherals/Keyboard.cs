using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace VzEmulator.Peripherals
{
    class Keyboard : IPeripheral
    {
        public Tuple<ushort, ushort> PortRange => null;
        public Tuple<ushort, ushort> MemoryRange { get; } = new Tuple<ushort, ushort>(VzConstants.OutputLatchAndKbStart, VzConstants.OutputLatchAndKbEnd);

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IInterruptEnableFlag intSource;

        internal KeyState CurrentKeyState { get; set; }

        //public bool UseKeyQueue;
        public bool UseKeyQueue = true;

        private Queue<KeyState> KeyQueue = new Queue<KeyState>();

        public class KeyState {
            public int? Key;
            public Keys? KeyCode = Keys.None;
            public int TimesRead { get; set; }

            public KeyState(int? keyValue, Keys keyData)
            {
                Key = keyValue;
                KeyCode = keyData;
            }

            public bool IsKeyPressed => Key.HasValue || KeyCode != Keys.None;
            public char ToLower()
            {
                var key = Key;
                //Convert upper case keys to lowercase
                if (key >= 97 && key <= 122)
                    key -= 32;
                var keyChar = '\0';
                keyChar = key.HasValue ? (char)key.Value : '\0';
                return keyChar;
            }
        }

        public Keyboard(IInterruptEnableFlag interruptSource)
        {
            intSource = interruptSource;
        }

        public byte? HandleMemoryRead(ushort address)
        {
            //Get value from queue if in queue mode
            var keyState = CurrentKeyState;
            if (UseKeyQueue && KeyQueue?.Count > 0)
                keyState = KeyQueue.Peek();

            //Get lower 8 Bytes of address
            var addr = address & 0xff;
            //Invert address - key scanning is active low
            addr = 0xff - addr;
            //Default value is 111111, pressed keys are active low
            const byte keyMask= 63;
            byte value = 0b10111111;

            if (keyState?.IsKeyPressed ?? false)
            {
                var keyCode = keyState.KeyCode & Keys.KeyCode;
                var c = keyState.ToLower();
                
                //These are scanned in order by the ROM
                if ((addr & 1) > 0)
                {
                    if (c == 'R')
                        value &= 0b11011111;
                    if (c == 'Q')
                        value &= 0b11101111;
                    if (c == 'E')
                        value &= 0b11110111;
                    if (c == ' ')
                        value &= 0b11111011;
                    if (c == 'W')
                        value &= 0b11111101;
                    if (c == 'T')
                        value &= 0b11111110;
                }

                if ((addr & 2) > 0)
                {
                    if (c == 'F')
                        value &= 0b11011111;
                    if (c == 'A')
                        value &= 0b11101111;
                    if (c == 'D')
                        value &= 0b11110111;
                    if ((keyState.KeyCode & Keys.Control) == Keys.Control
                        || keyCode == Keys.Left || keyCode == Keys.Back
                        || keyCode == Keys.Right || keyCode == Keys.Up
                        || keyCode == Keys.Down || keyCode == Keys.Insert
                        || keyCode == Keys.Delete || keyCode == Keys.End)
                        value &= 0b11111011;
                    if (c == 'S')
                        value &= 0b11111101;
                    if (c == 'G')
                        value &= 0b11111110;
                }
                if ((addr & 4) > 0)
                {
                    if (c == 'V')
                        value &= 0b11011111;
                    if (c == 'Z')
                        value &= 0b11101111;
                    if (c == 'C')
                        value &= 0b11110111;
                    if ((keyState.KeyCode & Keys.Shift) == Keys.Shift ||
                        ((keyState.KeyCode & Keys.Control) != Keys.Control && (keyState.KeyCode & Keys.Shift) != Keys.Shift && (keyState.KeyCode & Keys.KeyCode) == Keys.Oemplus))
                        value &= 0b11111011;
                    if (c == 'X')
                        value &= 0b11111101;
                    if (c == 'B')
                        value &= 0b11111110;
                }
                if ((addr & 8) > 0)
                {
                    if (c == '4')
                        value &= 0b11011111;
                    if (c == '1')
                        value &= 0b11101111;
                    if (c == '3')
                        value &= 0b11110111;
                    if ((keyState.KeyCode & Keys.Alt) == Keys.Alt)
                        value &= 0b11111011;
                    if (c == '2')
                        value &= 0b11111101;
                    if (c == '5')
                        value &= 0b11111110;
                }
                if ((addr & 16) > 0)
                {
                    if (c == 'M' || keyCode == Keys.Left || keyCode == Keys.Back)
                        value &= 0b11011111;
                    if (c == ' ' || keyCode == Keys.Down)
                        value &= 0b11101111;
                    if (keyCode == Keys.Oemcomma
                        || keyCode == Keys.Right)
                        value &= 0b11110111;
                    if (keyCode == Keys.F1)
                        value &= 0b11111011;
                    if (keyCode == Keys.OemPeriod
                        || keyCode == Keys.Up)
                        value &= 0b11111101;
                    if (c == 'N')
                        value &= 0b11111110;
                }
                if ((addr & 32) > 0)
                {
                    if (c == '7')
                        value &= 0b11011111;
                    if (c == '0')
                        value &= 0b11101111;
                    if (c == '8')
                        value &= 0b11110111;
                    if (c == '-' || (keyState.KeyCode & Keys.KeyCode) == Keys.OemMinus || ((keyState.KeyCode & Keys.Shift) != Keys.Shift & (keyState.KeyCode & Keys.KeyCode) == Keys.Oemplus))
                        value &= 0b11111011;
                    if (c == '9')
                        value &= 0b11111101;
                    if (c == '6')
                        value &= 0b11111110;
                }
                if ((addr & 64) > 0)
                {
                    if (c == 'U')
                        value &= 0b11011111;
                    if (c == 'P')
                        value &= 0b11101111;
                    if (c == 'I')
                        value &= 0b11110111;
                    if (c == (char)13)
                        value &= 0b11111011;
                    if (c == 'O')
                        value &= 0b11111101;
                    if (c == 'Y')
                        value &= 0b11111110;
                }
                if ((addr & 128) > 0)
                {
                    if (c == 'J')
                        value &= 0b11011111;
                    if (c == ';' || keyCode == Keys.Delete || keyCode == Keys.Oem1 ||
                        ((keyState.KeyCode & Keys.Shift) == Keys.Shift && (keyState.KeyCode & Keys.KeyCode) == Keys.Oemplus))
                        value &= 0b11101111;
                    if (c == 'K')
                        value &= 0b11110111;
                    if (c == ':' || keyCode == Keys.End || keyCode == Keys.Oem7)
                        value &= 0b11111011;
                    if (c == 'L' || keyCode == Keys.Insert)
                        value &= 0b11111101;
                    if (c == 'H')
                        value &= 0b11111110;
                }
            }
            else
            {
                //keys scanned but no key pressed. Good place for a small delay to reduce host cpu use when then guest process is just waiting for input
                //System.Threading.Thread.Sleep(TimeSpan.Zero); //Do any other work waiting
            }

            if (intSource.IsEnabled)
            {
                value &= keyMask;
                intSource.IsEnabled = false;
            }

            //track key presses and de-queue key if in queue mode if it has been read more than once
            if (UseKeyQueue && KeyQueue?.Count > 0 && (value & keyMask)!=keyMask)
            {
                KeyQueue.Peek().TimesRead++;
                if (KeyQueue.Peek().TimesRead > 4) KeyQueue.Dequeue();
            }
            
            return value;
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
            //do nothing
        }

        internal void SetKeyState(KeyState keyState)
        {
            if (UseKeyQueue)
                if (keyState.IsKeyPressed)
                    KeyQueue.Enqueue(keyState);
            else
                CurrentKeyState = keyState;
        }
    }
}
