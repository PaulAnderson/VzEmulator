using System;
using System.Windows.Forms;


namespace VzEmulator.Peripherals
{
    class Keyboard : IPeripheral
    {
        public Tuple<ushort, ushort> PortRange => null;
        public Tuple<ushort, ushort> MemoryRange { get; } = new Tuple<ushort, ushort>(VzConstants.OutputLatchAndKbStart, VzConstants.OutputLatchAndKbEnd);

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IInterruptEnableFlag intSource;

        public int? currentKey;
        public Keys? currentKeyCode = Keys.None;

        public Keyboard(IInterruptEnableFlag interruptSource)
        {
            intSource = interruptSource;
        }

        public byte? HandleMemoryRead(ushort address)
        {
            //Get lower 8 Bytes of address
            var addr = address & 0xff;
            //Invert address - key scanning is active low
            addr = 0xff - addr;
            //Default value is 111111, pressed keys are active low
            byte value = 0b10111111;

            if (currentKey.HasValue || currentKeyCode != Keys.None)
            {
                var keyCode = currentKeyCode & Keys.KeyCode;

                //Convert upper case keys to lowercase
                if (currentKey >= 97 && currentKey <= 122)
                    currentKey -= 32;

                var c = '\0';
                c = currentKey.HasValue ? (char)currentKey.Value : '\0';

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
                    if ((currentKeyCode & Keys.Control) == Keys.Control
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
                    if ((currentKeyCode & Keys.Shift) == Keys.Shift ||
                        ((currentKeyCode & Keys.Control) != Keys.Control && (currentKeyCode & Keys.Shift) != Keys.Shift && (currentKeyCode & Keys.KeyCode) == Keys.Oemplus))
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
                    if ((currentKeyCode & Keys.Alt) == Keys.Alt)
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
                    if (c == '-' || (currentKeyCode & Keys.KeyCode) == Keys.OemMinus || ((currentKeyCode & Keys.Shift) != Keys.Shift & (currentKeyCode & Keys.KeyCode) == Keys.Oemplus))
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
                        ((currentKeyCode & Keys.Shift) == Keys.Shift && (currentKeyCode & Keys.KeyCode) == Keys.Oemplus))
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
                value &= 0x7f;
                intSource.IsEnabled = false;
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
    }
}
