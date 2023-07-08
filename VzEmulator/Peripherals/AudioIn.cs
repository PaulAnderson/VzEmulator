using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CSCore;
using CSCore.SoundOut;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Codecs.WAV;

namespace VzEmulator.Peripherals
{
    class AudioIn  : IClockSynced , IAudioInput, IAudioStreamIn
    {
        int samplesPerSecond = 22050;
        int bytesPerSample = 2;
        int targetBufferLength;

        decimal currentClockFrequencyMhz= VzConstants.ClockFrequencyMhz;

        byte CurrentValue;

        WaveFileReader AudioReader;
        Stream inputStream;
        Stream inputSoundStream;
        BinaryReader reader;
        public Stream InputStream { get => inputStream; set
            {
                inputStream = value;
                if (inputStream != null)
                {
                    AudioReader = new WaveFileReader(inputStream);
                    samplesPerSecond = AudioReader.WaveFormat.SampleRate;
                    bytesPerSample = AudioReader.WaveFormat.BitsPerSample / 8;
                    CalculateTargetTStates();
                     reader = new BinaryReader(inputStream);

                }
            }
        }

        decimal IClockSynced.ClockFrequency
        {
            get => currentClockFrequencyMhz;
            set { currentClockFrequencyMhz = value;
                CalculateTargetTStates();
            }   
        }

        bool IClockSynced.ClockSyncEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        int targetTStates;
        int tStateCount = 0;
        
        void CalculateTargetTStates()
        {
            targetTStates = (int)currentClockFrequencyMhz * 1000000 / samplesPerSecond * bytesPerSample;
            targetTStates = (int)(targetTStates * 1);
            targetBufferLength = samplesPerSecond / 64 * bytesPerSample;
        }

        ICpu cpu;
        public AudioIn(ICpu cpu=null)
        {
            this.cpu = cpu;
            if (cpu != null)
            {
                cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;
            }

            ((IClockSynced)this).ClockFrequency = VzConstants.ClockFrequencyMhz; //Set clock frequency to default, ensure targetTStates is set.

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
        //    ReadAndSyncronizeSound(periodLengthInCycles);
        }
        private void ReadAndSyncronizeSound(int clockCyclesElapsed)
        {
            tStateCount += clockCyclesElapsed;

            if (tStateCount >= targetTStates)
            {
                tStateCount = targetTStates - tStateCount;
                ProcessSoundInput();
            }
        }
        private void ProcessSoundInput() { 
            //advance postion in audio stream and read value into latch
            if (reader != null)
            {
                byte[] buffer = new byte[bytesPerSample];
                reader.Read(buffer, 0, buffer.Length);
                CurrentValue = (byte)(buffer[0]);
                if (CurrentValue > 128)
                {
                    CurrentValue = 0;
                }
                else
                {
                    CurrentValue = 255;
                }
               //Console.Write($"{CurrentValue} ");
                if (inputStream.Position >= inputStream.Length)
                {
                    reader.Dispose();
                    reader = null;
                    inputStream.Dispose();
                    inputStream = null;
                    AudioReader = null;
                }
            }

        }

        public byte HandleMemoryRead(ushort address)
        {
            if (inputStream!=null)
            {
          //      Console.Write($" [{cpu.Registers.PC:X4}] ");
            }
            //alternate method just read value here based on elapsed tstates and return it.
            return (byte)(CurrentValue & 0b11000000);
        }
    }
}
