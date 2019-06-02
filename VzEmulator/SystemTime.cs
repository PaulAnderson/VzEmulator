using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator
{
    interface SystemTime
    {
        DateTime Now { get; }
    }
    class SystemTimeDefaultImplementation : SystemTime
    {
        public DateTime Now => DateTime.Now;
    }
}
