using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    class Sound { 
  
        OutputLatch outputLatch;
        ICpu cpu;
        SystemTime systemTime;

        const int speakerBit = 1; //1,32 = speaker. 2,4 = tape out
        private const int z80targetKips = 514; //540/4*3.5469

        private int targetCycleLengthMs = 300;
        int instructionCount = 0;
        DateTime? cycleStartTime;
        List<byte> outputLatchHistory;

        public Sound(OutputLatch outputLatch, ICpu cpu, SystemTime systemTime = null)
        {
            this.systemTime = systemTime ?? new SystemTimeDefaultImplementation();
            this.outputLatch = outputLatch;
            this.cpu = cpu;
            cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;
        }

        private int prevValue;
        private void Cpu_AfterInstructionExecution(object sender, InstructionEventArgs e)
        {
            if (cycleStartTime == null && prevValue == (outputLatch.Value & speakerBit))
            {
                prevValue = outputLatch.Value & speakerBit;
                return;
            }
            prevValue = outputLatch.Value & speakerBit;

            if (cycleStartTime == null) StartCycle();

            instructionCount++;
            outputLatchHistory.Add(outputLatch.Value);

            var elapsedMs = (systemTime.Now - cycleStartTime.Value).TotalMilliseconds;
            if (elapsedMs > targetCycleLengthMs)
            {
                GenerateSound(elapsedMs, outputLatchHistory);
                cycleStartTime = null;//wait for next transition
            }
            
        }
        private void StartCycle()
        {
            outputLatchHistory = new List<byte>(instructionCount);
            instructionCount = 0;
            cycleStartTime = systemTime.Now;
        }
        private void GenerateSound(double elapsedMs, List<byte> latchData)
        {
            //todo:
            //scale history to sampleRate / (1s / elapsedMs)
            //Create a wav fie in memory
            //queue the sound play the sound on a new thread
            //check queue length, if >.5 sec then cull 
            int samplesPerSecond = 96000;
            short bitsPerSample = 16;

            MemoryStream stream = new MemoryStream(samplesPerSecond*bitsPerSample/8);
            BinaryWriter writer = new BinaryWriter(stream);
            int RIFF = 0x46464952;
            int WAVE = 0x45564157;
            int formatChunkSize = 16;
            int headerSize = 8;
            int format = 0x20746D66;
            short formatType = 1;
            short tracks = 1;
            short frameSize = (short)(tracks * ((bitsPerSample + 7) / 8));
            int bytesPerSecond = samplesPerSecond * frameSize;
            int waveSize = 4;
            int data = 0x61746164;
            int samples = (int)(samplesPerSecond/(1000/elapsedMs));
            int dataChunkSize = samples * frameSize;
            int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;
            writer.Write(RIFF);
            writer.Write(fileSize);
            writer.Write(WAVE);
            writer.Write(format);
            writer.Write(formatChunkSize);
            writer.Write(formatType);
            writer.Write(tracks);
            writer.Write(samplesPerSecond);
            writer.Write(bytesPerSecond);
            writer.Write(frameSize);
            writer.Write(bitsPerSample);
            writer.Write(data);
            writer.Write(dataChunkSize);

            double SampleRateRatio = (double)samplesPerSecond / latchData.Count / ( 1000 / elapsedMs);
            double cpuSpeedRatio = latchData.Count / (elapsedMs / 1000) / (z80targetKips*1000);
            double totalRatio = SampleRateRatio * cpuSpeedRatio;

            if (totalRatio < 1)
            {

                totalRatio = 1 / totalRatio;

                for (double i = 0; i < latchData.Count; i += totalRatio)
                {
                    short s;
                    s = (short) ((latchData[(int) i] & speakerBit) / speakerBit * 1000);
                    writer.Write(s);
                }
            }
            else
            {
                //todo. cpu running too slow for realtime sound probably
            }

            stream.Seek(0, SeekOrigin.Begin);
            var player = new SoundPlayer(stream);
            player.Play();
            stream.Close();

        }

    }
}
