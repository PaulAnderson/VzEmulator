using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    internal interface ILatchValue
    {
        Byte Value { get; set; } //Set is private use except restoring state from saved file
    }
}
