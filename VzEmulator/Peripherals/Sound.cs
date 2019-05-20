using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    class Sound { 
  
        OutputLatch outputLatch;
        ICpu cpu;
        SystemTime systemTime;

        int targetCycleLengthMs = 100;
        int samplesPerSecond = 44100;

        int instructionCount=0;
        DateTime? cycleStartTime;
        List<byte> outputLatchHistory;

        public Sound(OutputLatch outputLatch, ICpu cpu, SystemTime systemTime = null)
        {
            this.systemTime = systemTime ?? new SystemTimeDefaultImplementation();
            this.outputLatch = outputLatch;
            this.cpu = cpu;
            cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;
        }

        private void Cpu_AfterInstructionExecution(object sender, InstructionEventArgs e)
        {
            if (cycleStartTime == null) StartCycle();

            instructionCount++;
            outputLatchHistory.Add(outputLatch.Value);

            var elapsedMs = (systemTime.Now - cycleStartTime.Value).TotalMilliseconds;
            if (elapsedMs > targetCycleLengthMs)
            {
                GenerateSound(elapsedMs, outputLatchHistory);
                StartCycle();
            }
            
        }
        private void StartCycle()
        {
            outputLatchHistory = new List<byte>(instructionCount);
            instructionCount = 0;
            cycleStartTime = systemTime.Now;
        }
        private void GenerateSound(double elapsedMs, List<byte> data)
        {
            //todo:
            //scale history to sampleRate / (1s / elapsedMs)
            //Create a wav fie in memory
            //queue the sound play the sound on a new thread
            //check queue length, if >.5 sec then cull 
        }

    }
}
