using System;

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
