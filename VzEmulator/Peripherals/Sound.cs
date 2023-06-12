using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CSCore;
using CSCore.SoundOut;
using CSCore.Streams;

namespace VzEmulator.Peripherals
{
    class Sound
    {
        private readonly MemoryLatch outputLatch;

        BinaryWriter writer;

        const int SpeakerBit1 = 1; //1,32 = speaker. 2,4 = tape out
        const int SpeakerBit2 = 32; //1,32 = speaker. 2,4 = tape out

        const int samplesPerSecond = 32000;
        const int bytesPerSample = 2;
        const int targetBufferLength = samplesPerSecond/32*bytesPerSample;

        const double VzClockFrequencyMhz = 3.546900; //Todo move to constants
        public bool SoundEnabled { get; set; } = false;
        public bool TestTone { get; set; } = false;

        WriteableBufferingSource buffer;

        int targetTStates = (int)VzClockFrequencyMhz * 1000000 / samplesPerSecond * bytesPerSample;// clock frequency is 3.54MHz,divided by audio sample rate
        int tStateCount = 0;
        public Sound(MemoryLatch outputLatch, ICpu cpu, SystemTime systemTime = null)
        {
            this.outputLatch = outputLatch;
            cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;

            var source = new WriteableBufferingSource(new WaveFormat(samplesPerSecond, 8, 1, AudioEncoding.Pcm)) { FillWithZeros = true };
            var soundOut = GetSoundOut();
            soundOut.Initialize(source);
            soundOut.Play();

            buffer = source;

            //Prefill ~1/3 of a second of sound
            byte[] fillBuffer = Enumerable.Repeat((byte)0x7F, targetBufferLength).ToArray();
            buffer.Write(fillBuffer, 0, fillBuffer.Length);

        }

        private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                return new WasapiOut();
            else
                return new DirectSoundOut();
        }

        private void Cpu_AfterInstructionExecution(object sender, InstructionEventArgs e)
        {
            tStateCount += e.TStates;

            if (tStateCount >= targetTStates)
            {
                tStateCount = targetTStates - tStateCount;
                ProcessSoundAfterCpuInstruction();
            }

            //if buffer getting full, busy wait to slow cpu down
            //todo move to cpu. For now, the cpu speed is tied to the sound buffer size which shrinks at sampleRate - 32k per second
            if (buffer.Length > targetBufferLength * 1.2)
            {
                while (buffer.Length > targetBufferLength)
                {
                    Thread.Sleep(0);
                }
            }
        }

        byte Previousvalue = 0x7f;
        private void ProcessSoundAfterCpuInstruction()
        {

                byte value = 0x7F;
                bool value1 = (outputLatch.Value & SpeakerBit1) > 0;
                bool value2 = (outputLatch.Value & SpeakerBit2) > 0;

                if (value1 && !value2) value = 0xff;
                if (value2 && !value1) value = 0x00;

                var smoothedValue = (byte)((value + Previousvalue) / 2);

                var b = new byte[bytesPerSample] {
                    smoothedValue,
                    value,
                };

                Previousvalue = smoothedValue;

                //test - generate continuous tones to test the buffering
                if (TestTone)
                {
                    GenerateTestTone(b);
                }
                //end test code


                if (!SoundEnabled)
                    for (var x = 0; x < bytesPerSample; x++)
                        b[x] = 0x7F;

                buffer.Write(b, 0, bytesPerSample);

        }

        //test tone variables 
        double sinPos = 0;
        double sinPos2 = 0;
        private void GenerateTestTone(byte[] b)
        {
            double modulatedValue = Math.Sin(sinPos) * Math.Sin(sinPos2);
            double amplitude = (modulatedValue + 1) * 128;
            b[0] = (byte)((b[0] ^ (byte)amplitude)  );
            b[1] = (byte)((b[0] ^ (byte)amplitude)  );

            sinPos += 0.1;
            sinPos2 += 0.21;
            if (sinPos2 > Math.PI * 2)
                sinPos2 = 0;
            if (sinPos > Math.PI * 2)
                sinPos = 0;
        }
 
    }
}
