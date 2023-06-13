using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator
{
    internal class Z80DotNetClockSynchronizer : IClockSynchronizer
    {
        decimal clockFrequency;
        private IClockSynced clockSyncedObject;
        public IClockSynced ClockSyncedObject
        {
            get => clockSyncedObject;
            set
            {
                clockSyncedObject = value;
                if (value != null) value.ClockFrequency = clockFrequency;
            }
        }
        decimal IClockSynchronizer.EffectiveClockFrequencyInMHz { 
            get => clockFrequency; 
            set   { clockFrequency = value;
                    if (clockSyncedObject!= null) clockSyncedObject.ClockFrequency = value;
            } 
        }

        void IClockSynchronizer.Start()
        {
        }

        void IClockSynchronizer.Stop()
        {
        }

        void IClockSynchronizer.TryWait(int periodLengthInCycles)
        {
            clockSyncedObject?.ProcessClockCycles(periodLengthInCycles);
        }
    }
}
