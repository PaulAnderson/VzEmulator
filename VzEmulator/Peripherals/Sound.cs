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

        int targetCycleLengthMs = 1000;
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

        const int speakerBit = 2; //1,32 = speaker. 2,4 = tape out

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
                //StartCycle();
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

            FileStream stream = new FileStream("test.wav", FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);
            int RIFF = 0x46464952;
            int WAVE = 0x45564157;
            int formatChunkSize = 16;
            int headerSize = 8;
            int format = 0x20746D66;
            short formatType = 1;
            short tracks = 1;
            int samplesPerSecond = 44100;
            short bitsPerSample = 16;
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

            double ratio = (double)samplesPerSecond / (1000 / elapsedMs);

            for (double i = 0; i < samples ; i+=ratio)
            {
                short s;
                if (i < latchData.Count)
                {
                    s = (short)((latchData[(int)i] & speakerBit+4)/ speakerBit * 32000);
                    writer.Write(s);

                }
                else
                    break;
            }

            writer.Close();
            stream.Close();

            var player = new SoundPlayer();
            player.SoundLocation = "test.wav";

            player.Play();
        }

    }
}
