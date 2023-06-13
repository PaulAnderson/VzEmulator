using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using CSCore.Streams;

namespace VzEmulator.Peripherals
{
    class AudioOut  : IClockSynced
    {
        private readonly MemoryLatch outputLatch;

        static readonly (byte,byte) SpeakerBits = (1,32); //1,32 = speaker. 2,4 = tape out
        static readonly (byte,byte) TapeBits = (2,4); //1,32 = speaker. 2,4 = tape out

        const int samplesPerSecond = 22050;
        const int bytesPerSample = 1;
        const int targetBufferLength = samplesPerSecond/64*bytesPerSample;

        decimal currentClockFrequencyMhz= VzConstants.ClockFrequencyMhz;
        public bool SoundEnabled { get; set; } = false;
        public bool TestTone { get; set; } = false;
        decimal IClockSynced.ClockFrequency
        {
            get => currentClockFrequencyMhz;
            set { currentClockFrequencyMhz = value;
                  targetTStates = (int)currentClockFrequencyMhz * 1000000 / samplesPerSecond * bytesPerSample;
            }
        }

        private IWriteable _writer;

        WriteableBufferingSource buffer;

        int targetTStates;
        int tStateCount = 0;
        //Sound class. Use cpu parameter to sync sound to cpu using the AfterInstructionExecutionEvent. If null, sound will be synced to clock using IClockSynced.ProcessClockCycle
        //Todo implement cassette in/out
        public AudioOut(MemoryLatch outputLatch, ICpu cpu=null)
        {
            this.outputLatch = outputLatch;
            if (cpu != null)
            {

                cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;
            }

            ((IClockSynced)this).ClockFrequency = VzConstants.ClockFrequencyMhz; //Set clock frequency to default, ensure targetTStates is set.

            var source = new WriteableBufferingSource(new WaveFormat(samplesPerSecond, 8, 1, AudioEncoding.Pcm)) { FillWithZeros = true };
            var soundOut = GetSoundOut();
            soundOut.Initialize(source);
            soundOut.Play();

            buffer = source;

            //Prefill buffer
            byte[] fillBuffer = Enumerable.Repeat((byte)0x7F, targetBufferLength*2).ToArray();
            buffer.Write(fillBuffer, 0, fillBuffer.Length);

            //Test writing to wav file
            _writer = new WaveWriter("test.wav", new WaveFormat(samplesPerSecond, 8, 1, AudioEncoding.Pcm));
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
            byte value = GetAudioValue(SpeakerBits);
            byte tapeValue = GetTapeValue(TapeBits);

            //value ^= tapeValue;

            var smoothedValue = (byte)((value + Previousvalue) / 2);

            var b = new byte[bytesPerSample] {
                    smoothedValue,
                //    value,
                };

            Previousvalue = smoothedValue;

            //test - generate continuous tones to test the buffering
            if (TestTone)
            {
                GenerateTestTone(b);
            }
            //end test code


            ////test writing cassette data to wav file
            //if (tapeValue != 0)
            //    cassetteOutputEnabled = true;
            //if (cassetteOutputEnabled)
            //{
            //    _writer.Write(b, 0,1);
            //}
            //end test

            if (!SoundEnabled)
                for (var x = 0; x < bytesPerSample; x++)
                    b[x] = 0x7F;

            buffer.Write(b, 0, bytesPerSample);

        }

        private byte GetAudioValue((byte,byte) bitMasks)
        {
            byte value = 0x7F;
            bool value1 = (outputLatch.Value & bitMasks.Item1) > 0;
            bool value2 = (outputLatch.Value & bitMasks.Item2) > 0;

            if (value1 && !value2) value = 0xff;
            if (value2 && !value1) value = 0x00;
            return value;
        }
        private byte GetTapeValue((byte, byte) bitMasks)
        {
            byte value = (byte)((outputLatch.Value & 6) << 5);
            return value;
        }

        //test tone variables 
        double sinPos = 0;
        double sinPos2 = 0;
        private bool cassetteOutputEnabled;

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
