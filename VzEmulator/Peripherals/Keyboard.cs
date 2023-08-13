using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace VzEmulator.Peripherals
{
    //Todo split latch, keyboard, cassette in into separate classes
    public class Keyboard : IPeripheral, IClockSynced
    {
        public Tuple<ushort, ushort> PortRange => null;
        public Tuple<ushort, ushort> MemoryRange { get; } = new Tuple<ushort, ushort>(VzConstants.OutputLatchAndKbStart, VzConstants.OutputLatchAndKbEnd);

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IInterruptEnableFlag intSource;

        internal KeyState CurrentKeyState { get; set; }
        public decimal ClockFrequency { get; set; } //not used
        public bool ClockSyncEnabled { get; set; }

        int currentClockCycles; //Roll over every 3,5400,000 cycles

        public bool UseKeyQueue = true;

        private Queue<KeyState> KeyQueue = new Queue<KeyState>();

        public class KeyState {
            public int? Key;
            public Keys? KeyCode = Keys.None;
            public int TimesRead { get; set; }
            public byte KeyScanMask { get; set; }

            //The cpu cycle when the key was recorded. Used to age the queue
            public int CycleTimeStamp { get; set; }

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
            const byte keyMask= 0b00111111; //63;
            byte value = 0b00111111;

            if (keyState?.IsKeyPressed ?? false)
            {
                value = GetKeyMatrixValue(keyState, addr, value);

            }
            else
            {
                //keys scanned but no key pressed. Good place for a small delay to reduce host cpu use when then guest process is just waiting for input
                //System.Threading.Thread.Sleep(TimeSpan.Zero); //Do any other work waiting
            }

            //if (intSource.IsEnabled || KeyQueue?.Count > 0)
            //{
            //    value &= keyMask;
            //    intSource.IsEnabled = false;
            //}

            //track key presses and de-queue key if in queue mode if it has been read more than once
            //todo track emulated time and release key after set number of clock cycles
            const bool useCpuCycleBasedKeyAging = true;
            int keyHoldTime = (int)(ClockFrequency * 100000); // 100ms
            if (useCpuCycleBasedKeyAging)
            {
                if (KeyQueue.Count > 0)
                {
                    //Age keys based on number of clock cycles
                    var queuedKeyCycleTimeStamp = KeyQueue.Peek().CycleTimeStamp;

                    //Handle wrap around
                    if (queuedKeyCycleTimeStamp > currentClockCycles)
                        KeyQueue.Peek().CycleTimeStamp = currentClockCycles;

                    if (currentClockCycles - KeyQueue.Peek().CycleTimeStamp > keyHoldTime)
                    {
                        //todo set next key in queue at current cycletimestamp?
                        KeyQueue.Dequeue();
                        if (KeyQueue.Count> 0 && KeyQueue.Peek().CycleTimeStamp==-1)
                        {
                            if (KeyQueue.Peek().IsKeyPressed == false)
                            {
                                //'No key' queued. delay
                                KeyQueue.Peek().CycleTimeStamp = currentClockCycles ;
                            } else
                            {
                                KeyQueue.Peek().CycleTimeStamp = currentClockCycles - keyHoldTime + keyHoldTime / 4;
                            }
                        }

                    }
                }
            }
            else
            {
                //Age keys based on number of times the keyboard is scanned
                if (UseKeyQueue && KeyQueue?.Count > 0 && (value & keyMask) != keyMask)
                {
                    KeyQueue.Peek().TimesRead++;
                    if (KeyQueue.Peek().TimesRead > 7) KeyQueue.Dequeue();
                }
                //de-queue key if it has been read on all address lines and not inputted
                if (UseKeyQueue && KeyQueue?.Count > 0 && value == keyMask)
                {
                    if (addr < 0xff)
                        KeyQueue.Peek().KeyScanMask |= (byte)(addr);
                    else
                        KeyQueue.Peek().TimesRead++;
                    if (KeyQueue.Count > 0 && KeyQueue.Peek().KeyScanMask == 0xff && KeyQueue.Peek().Key.Value != 13)
                        KeyQueue.Dequeue();
                    if (KeyQueue.Count > 0 && KeyQueue.Peek().TimesRead > 10)
                        KeyQueue.Dequeue();
                }
            }
            
            return value;
        }

        private static byte GetKeyMatrixValue(KeyState keyState, int addr, byte value)
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

            return value;
        }

        public bool IsKeyValueVZKey(KeyState keyState) => GetKeyMatrixValue(keyState, 0xff, 0xff) < 0xff;

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
            {
                keyState.CycleTimeStamp = currentClockCycles;
                if (keyState.IsKeyPressed && IsKeyValueVZKey(keyState))
                    KeyQueue.Enqueue(keyState);
            }
            else
            {
                CurrentKeyState = keyState;
            }
            
        }

        //Used to queue up pasted text, a text file etc
        internal void QueueKeys(string keysToQueue)
        {
            //Clear current queue
            KeyQueue?.Clear();

            KeyState previousKey = new KeyState(0, 0);
            KeysConverter kc = new KeysConverter();
            //iterate over each char in keysToQueue, adding each one to KeyQueue after converting it with kc
            for (var i = 0; i < keysToQueue.Length; i++)
            {
                char c = keysToQueue[i];
                var pauseAfterKey = false;
                var keyCode = new Keys(); //todo apply shift, etc
                if (c == '@') { keyCode |= Keys.Shift; c = '0'; pauseAfterKey = true; }
                if (c == '!') { keyCode |= Keys.Shift; c = '1'; pauseAfterKey = true; }
                if (c == '"') { keyCode = Keys.Shift;  c = '2'; pauseAfterKey = true; }
                if (c == '#') { keyCode |= Keys.Shift; c = '3'; pauseAfterKey = true; }
                if (c == '$') { keyCode = Keys.Shift; c = '4'; pauseAfterKey = true; }
                if (c == '%') { keyCode |= Keys.Shift; c = '5'; pauseAfterKey = true; }
                if (c == '&') { keyCode |= Keys.Shift; c = '6'; pauseAfterKey = true; }
                if (c == '\'') { keyCode |= Keys.Shift; c = '7'; pauseAfterKey = true; }
                if (c == '(') { keyCode |= Keys.Shift; c = '8'; pauseAfterKey = true; }
                if (c == ')') { keyCode |= Keys.Shift; c = '9'; pauseAfterKey = true; }
                if (c == '^') { keyCode |= Keys.Shift; c = 'n'; pauseAfterKey = true; } //Up arrow symbol
                if (c == '|') { keyCode |= Keys.Shift; c = 'n'; pauseAfterKey = true; } //Up arrow symbol
                if (c == '[') { keyCode |= Keys.Shift; c = 'o'; pauseAfterKey = true; } 
                if (c == ']') { keyCode |= Keys.Shift; c = 'p'; pauseAfterKey = true; }
                if (c == '/') { keyCode |= Keys.Shift; c = 'k'; pauseAfterKey = true; }
                if (c == '?') { keyCode |= Keys.Shift; c = 'l'; pauseAfterKey = true; }
                if (c == '\\') { keyCode |= Keys.Shift; c = 'm'; pauseAfterKey = true; }
                if (c == '<') { keyCode |= Keys.Shift | Keys.Oemcomma; pauseAfterKey = true; }
                if (c == '>') { keyCode |= Keys.Shift | Keys.OemPeriod;  pauseAfterKey = true; }

                if (c == '+') { keyCode |= Keys.Shift | Keys.Oemplus; pauseAfterKey = true; }
                if (c == ',') { keyCode = Keys.Oemcomma; pauseAfterKey = true; }
                if (c == '=') { keyCode = Keys.Oemplus; pauseAfterKey = true; }

                if (c == '*')
                {
                    keyCode = Keys.Shift | Keys.Oem7;//todo encode c and keycode
                    c = ':';
                }

                //Queue the key. The first key press is at the current cpu cycle, the rest are at -1 so that they are re-timed after each key is pressed
                    var cycleTimeStamp = -1;
                if (i == 0)
                    cycleTimeStamp = currentClockCycles;
                var keyState = new KeyState(c, keyCode) { CycleTimeStamp = cycleTimeStamp };
                
                //Handle double letters
                if (previousKey.Key.HasValue && previousKey.Key.Value == keyState.Key.Value) 
                { 
                    KeyQueue.Enqueue(new KeyState(null, Keys.None) { CycleTimeStamp = cycleTimeStamp});
                }
                previousKey = keyState;

                //queue the key
                KeyQueue.Enqueue(keyState) ;

                //pause after shift
                if ((keyCode | Keys.Shift) == Keys.Shift && pauseAfterKey)
                {
                    KeyQueue.Enqueue(new KeyState(null, Keys.None) { CycleTimeStamp = cycleTimeStamp });
                }
            }
        }
        public void Reset()
        {
            KeyQueue?.Clear();
        }

        //IClockSync. Used to age key press events
        public void ProcessClockCycles(int periodLengthInCycles)
        {
            int maxClockCycles = (int)(((IClockSynced)this).ClockFrequency * 1000000);
            currentClockCycles += periodLengthInCycles;
            if (currentClockCycles > maxClockCycles)
            {
                currentClockCycles -= maxClockCycles;
            }
        }
    }
}
