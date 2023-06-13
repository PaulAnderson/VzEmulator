using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CSCore;
using CSCore.SoundOut;
using CSCore.SoundIn;
using CSCore.Streams;

namespace VzEmulator.Peripherals
{
    class AudioIn  : IClockSynced
    {
        int samplesPerSecond = 32000;
        int bytesPerSample = 2;
        int targetBufferLength;

        decimal currentClockFrequencyMhz= VzConstants.ClockFrequencyMhz;
        public bool SoundEnabled { get; set; } = false;

        decimal IClockSynced.ClockFrequency
        {
            get => currentClockFrequencyMhz;
            set { currentClockFrequencyMhz = value;
                  targetTStates = (int)currentClockFrequencyMhz * 1000000 / samplesPerSecond * bytesPerSample;
                targetBufferLength = samplesPerSecond / 64 * bytesPerSample;
            }   
        } 

        int targetTStates;
        int tStateCount = 0;
        

        //Sound class. Use cpu parameter to sync sound to cpu using the AfterInstructionExecutionEvent. If null, sound will be synced to clock using IClockSynced.ProcessClockCycle
        public AudioIn(ICpu cpu=null)
        {
            if (cpu != null)
            {
                cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;
            }

            ((IClockSynced)this).ClockFrequency = VzConstants.ClockFrequencyMhz; //Set clock frequency to default, ensure targetTStates is set.

          //todo
        }

        private ISoundIn GetSoundIn()
        {
            if (WasapiCapture.IsSupportedOnCurrentPlatform)
                return new WasapiCapture();
            else
                return new WaveIn();
        }

        private void Cpu_AfterInstructionExecution(object sender, InstructionEventArgs e)
        {
            ReadAndSyncronizeSound(e.TStates);
        }
        void IClockSynced.ProcessClockCycles(int periodLengthInCycles)
        {
            ReadAndSyncronizeSound(periodLengthInCycles);
        }
        private void ReadAndSyncronizeSound(int clockCyclesElapsed)
        {
            tStateCount += clockCyclesElapsed;

            if (tStateCount >= targetTStates)
            {
                tStateCount = targetTStates - tStateCount;
                 //todo
            }

        }

    }
}
