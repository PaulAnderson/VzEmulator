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
    internal class AudioOut : IClockSynced, IAudioOutput
    {
        private readonly MemoryLatch outputLatch;

        static readonly (byte, byte) SpeakerBits = (1, 32); //1,32 = speaker. 2,4 = tape out
        static readonly (byte, byte) TapeBits = (2, 4); //1,32 = speaker. 2,4 = tape out

        const int samplesPerSecond = 22050;
        const int bytesPerSample = 1;
        const int targetBufferLength = samplesPerSecond / 64 * bytesPerSample;

        decimal currentClockFrequencyMhz = VzConstants.ClockFrequencyMhz;
        public bool SoundEnabled { get; set; } = false;
        public bool TestTone { get; set; } = false;
        
        decimal IClockSynced.ClockFrequency
        {
            get => currentClockFrequencyMhz;
            set { currentClockFrequencyMhz = value;
                CalculateTargetTStates();

            }
        }

        bool clockSyncEnabled = true;
        bool IClockSynced.ClockSyncEnabled
        { get => clockSyncEnabled; set   {
                clockSyncEnabled = value;
                    } }

        void CalculateTargetTStates()
        {
            targetTStates = (int)currentClockFrequencyMhz * 1000000 / samplesPerSecond * bytesPerSample;
            if (currentClockFrequencyMhz == 4.0m) {
                //Override the CPU component default of 4.0mhz to 3.54mhz
                targetTStates = (int)(targetTStates / 1.16);
            }
        }
        public Stream OutputStream1 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private Stream cassetteOutStream;
        public Stream OutputStream2
        {
            get => cassetteOutStream;
            set {
                cassetteOutputEnabled = false;
                if (cassetteOutStream != null)
                    cassetteOutStream.Dispose();
                cassetteOutStream = value;
                if (cassetteOutStream != null)
                    CassetteWriter = new WaveWriter(cassetteOutStream, new WaveFormat(samplesPerSecond, 8, 1, AudioEncoding.Pcm));
            }
        }

        public void StartRecordStream2(string fileName)
        {
            OutputStream2 = File.OpenWrite(fileName);
        }
        
        public bool MixStream2 { get; set; }

        private bool cassetteOutputEnabled;
        private int cassetteOutputLastUpdate; //number of audio samples cassette value has been unchanged

        public event EventHandler<EventArgs> Stream1Started;
        public event EventHandler<EventArgs> Stream2Started;
        public event EventHandler<EventArgs> Stream2Idle;

        private IWriteable CassetteWriter;
        WriteableBufferingSource SpeakerWriter;

        int targetTStates;
        int tStateCount = 0;

        //todo pipe audio input to output for monitoring
        IAudioInput audioInput;

        private Drive drive;
        //Sound class. Use cpu parameter to sync sound to cpu using the AfterInstructionExecutionEvent. If null, sound will be synced to clock using IClockSynced.ProcessClockCycle
        public AudioOut(MemoryLatch outputLatch, IAudioInput audioInput, ICpu cpu=null )
        {
            this.audioInput = audioInput;
            this.outputLatch = outputLatch;
            if (cpu != null)
            {

                cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;
            }

            ((IClockSynced)this).ClockFrequency =  VzConstants.ClockFrequencyMhz; //Set clock frequency to default, ensure targetTStates is set.

            var source = new WriteableBufferingSource(new WaveFormat(samplesPerSecond, 8, 1, AudioEncoding.Pcm)) { FillWithZeros = true };
            var soundOut = GetSoundOut();
            soundOut.Initialize(source);
            soundOut.Play();

            SpeakerWriter = source;

            //Prefill buffer
            byte[] fillBuffer = Enumerable.Repeat((byte)0x7F, targetBufferLength*2).ToArray();
            SpeakerWriter.Write(fillBuffer, 0, fillBuffer.Length);

            //Test writing to wav file
            //CassetteWriter = new WaveWriter("test.wav", new WaveFormat(samplesPerSecond, 8, 1, AudioEncoding.Pcm));
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
            if (!clockSyncEnabled)
                return; //dont buffer sound when not running in realtime

            tStateCount += clockCyclesElapsed;

            if (tStateCount >= targetTStates)
            {
                tStateCount = targetTStates - tStateCount;
                ProcessSoundAfterCpuInstruction();
            }

            //if buffer getting full, busy wait to slow cpu down
            //todo move to cpu. For now, the cpu speed is tied to the sound buffer size which shrinks at sampleRate - 32k per second
            if (SpeakerWriter.Length > targetBufferLength * 1.2 & clockSyncEnabled)
            {
                while (SpeakerWriter.Length > targetBufferLength)
                {
                    Thread.Sleep(0);
                }
            }
        }

        byte windowSizeSpeaker = 2;
        byte rollingAverageSpeaker = 0x7f;
        byte windowSizeCassette = 2;
        byte rollingAverageCassette = 0x7f;
        byte lastTapeValue = 0x7f;
        private void ProcessSoundAfterCpuInstruction()
        {
            //Get audio stream byte values batched on latch contents
            byte value = GetAudioValue(SpeakerBits);
            byte tapeValue = GetTapeValue(TapeBits);

            var audioIn = audioInput.HandleMemoryRead(0);
            value ^= audioIn;

            if (!SoundEnabled)
                value = 0;

            //Mix cassette with normal speaker output if enabled
            if (MixStream2)
                value ^= tapeValue;
            
            //Calculate rolling average of value
            rollingAverageSpeaker = (byte)((rollingAverageSpeaker * (windowSizeSpeaker - 1) + value) / windowSizeSpeaker);
            rollingAverageCassette = (byte)((rollingAverageCassette * (windowSizeCassette - 1) + tapeValue) / windowSizeCassette);

            var SpeakerOutputBuffer = new byte[bytesPerSample]
            {
                rollingAverageSpeaker
            };
            var CassetteOutputBuffer = new byte[bytesPerSample]
            {
                rollingAverageCassette
            };

            //test - generate continuous tones to test the buffering
            if (TestTone)
            {
                GenerateTestTone(SpeakerOutputBuffer);
            }
            //end test code


            // write cassette data to wav file
            if (tapeValue != 0)
            {
                //raise event if casseteOutput changing from false to true
                if (!cassetteOutputEnabled)
                {
                    cassetteOutputEnabled = true;
                    Stream2Started?.Invoke(this, new EventArgs());
                }
            }
            if (cassetteOutputEnabled && CassetteWriter !=null)
            {
                CassetteWriter.Write(CassetteOutputBuffer, 0, 1);

                //Detect end of output
                if (tapeValue == lastTapeValue)
                {
                    cassetteOutputLastUpdate += 1;
                } else
                {
                    cassetteOutputLastUpdate = 0;
                }
                if (cassetteOutputLastUpdate>1000) //todo configurable value;
                {
                    cassetteOutputLastUpdate = 0;
                    cassetteOutputEnabled = false;
                    Stream2Idle?.Invoke(this, new EventArgs());
                }
                lastTapeValue = tapeValue;
            }

            SpeakerWriter.Write(SpeakerOutputBuffer, 0, bytesPerSample);

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

        private void GenerateTestTone(byte[] b)
        {
            double modulatedValue = Math.Sin(sinPos) * Math.Sin(sinPos2);
            double amplitude = (modulatedValue + 1) * 128;
            b[0] = (byte)((b[0] ^ (byte)amplitude)  );

            sinPos += 0.1;
            sinPos2 += 0.21;
            if (sinPos2 > Math.PI * 2)
                sinPos2 = 0;
            if (sinPos > Math.PI * 2)
                sinPos = 0;
        }


    }
}
