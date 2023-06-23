using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator
{
    internal interface IAudioStreamIn
    {
        Stream InputStream { get; set; }
    }
}
