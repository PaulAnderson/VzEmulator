using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    internal class Printer : IPeripheral , IPrinterOutput
    {
        Tuple<ushort, ushort> IPeripheral.PortRange => new Tuple<ushort, ushort>(0x0E, 0x0E);
        Tuple<ushort, ushort> IPeripheral.MemoryRange => null;
        bool IPeripheral.DebugEnabled { get; set; }

        List<string> IPrinterOutput.PrintedLines => _printedLines;
        readonly List<string> _printedLines = new List<string>();
        readonly StringBuilder lineStringBuilder = new StringBuilder(64); //assume max line length of 64

        public event EventHandler LinePrinted;

        byte? IPeripheral.HandleMemoryRead(ushort address)
        {
            return null;
        }

        bool IPeripheral.HandleMemoryWrite(ushort address, byte value)
        {
            return false;
        }

        byte? IPeripheral.HandlePortRead(ushort address)
        {
            return null;
        }

        void IPeripheral.HandlePortWrite(ushort address, byte value)
        {
            char charValue = Convert.ToChar(value);

            System.Console.WriteLine($"Printer Port Write. Addr {address}, value {value} char value {charValue}");
            
            if (charValue == '\r')
            {
                _printedLines.Add(lineStringBuilder.ToString());
                lineStringBuilder.Clear();
                OnLinePrinted();
            }
            else if (charValue == '\n')
            {
                //Ignore new line
            } else
            {
                lineStringBuilder.Append(charValue);
            }
        }

        protected virtual void OnLinePrinted()
        {
            LinePrinted?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            //do nothing
        }
    }
}
