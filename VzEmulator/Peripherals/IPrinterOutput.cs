using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    interface IPrinterOutput
    {
        List<string> PrintedLines { get; }
        event EventHandler LinePrinted;
    }
}
