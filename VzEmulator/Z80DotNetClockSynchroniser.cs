using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VzEmulator
{
    public class Z80DotNetClockSynchroniser : IClockSynchronizer
    {
        private const int MinTicksToWait = 0;

        private Stopwatch stopWatch = new Stopwatch();

        private decimal accummulatedTicks;

        public int BusyLoopTimePerCycle { get; set; } = 150;//todo calibration routine
        public decimal EffectiveClockFrequencyInMHz { get; set; }

        public void Start()
        {
            stopWatch.Reset();
            stopWatch.Start();
        }

        public void Stop()
        {
            stopWatch.Stop();
        }

        public void TryWait(int periodLengthInCycles)
        {
            for (int i = BusyLoopTimePerCycle*periodLengthInCycles; i > 0; i--) { }
            return;

            accummulatedTicks += (decimal)periodLengthInCycles / EffectiveClockFrequencyInMHz*10;
            decimal num = accummulatedTicks - (decimal)stopWatch.Elapsed.Ticks;
            accummulatedTicks = 5;
            if (num >= MinTicksToWait)
            {
                while (num > 0)
                {
                    // Busy wait until the desired time has elapsed
                    num = accummulatedTicks - (decimal)stopWatch.Elapsed.Ticks ;
                }

                accummulatedTicks = 0;
                stopWatch.Restart();
            }
        }
    }
}
