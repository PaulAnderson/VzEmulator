using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;

namespace VzEmulator.Peripherals
{
    class Sound
    {
        private readonly MemoryLatch outputLatch;
        private readonly ICpu cpu;
        private readonly SystemTime systemTime;
        BinaryWriter writer;

        const int SpeakerBit1 = 1; //1,32 = speaker. 2,4 = tape out
        const int SpeakerBit2 = 32; //1,32 = speaker. 2,4 = tape out

        const int samplesPerSecond = 32000;

        DateTime? cycleStartTime;

        int targetTicks;
        int targetBufferLength = 2000;
        int instructionCount;

        public bool SoundEnabled { get; set; } = false;
        public bool TestTone { get; set; } = false;

        WriteableBufferingSource buffer;

        // PID controller constants
        const double Kp = 0.1; // Proportional gain
        const double Ki = 0.000; // Integral gain
        const double Kd = 0.00; // Derivative gain

        // PID controller variables
        double integralSum = 0.0;
        double previousError = 0.0;

        public Sound(MemoryLatch outputLatch, ICpu cpu, SystemTime systemTime = null)
        {
            targetTicks = 10000000 / samplesPerSecond/2  ; //10 million ticks in a second
            this.systemTime = systemTime ?? new SystemTimeDefaultImplementation();
            this.outputLatch = outputLatch;
            this.cpu = cpu;
            cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;

            var source = new WriteableBufferingSource(new WaveFormat(samplesPerSecond, 8, 1,AudioEncoding.Pcm)) { FillWithZeros = true };
            var soundOut = GetSoundOut();
            soundOut.Initialize(source);
            soundOut.Play();
             
            buffer = source;
            
            //Prefill ~1/3 of a second of sound
            byte[] fillBuffer = new byte[targetBufferLength*2];
            buffer.Write(fillBuffer,0,fillBuffer.Length);
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
            if (!SoundEnabled)
                return;

            instructionCount++;

            //  if (instructionCount % 100 == 0 || buffer.Length > 20)
            //Console.WriteLine($"Buffer Length: {buffer.Length }, Position: {buffer.GetPosition().Ticks}");

            if ( instructionCount % 2 == 0 )
             ProcessSoundAfterCpuInstruction();
             
            var error =  targetBufferLength - buffer.Length ;

            // Calculate the PID output
            var output = Kp * error + Ki * integralSum + Kd * (error - previousError);

            // Adjust the target ticks based on the PID output
            targetTicks -= (int)output;

            integralSum += error;
            previousError = error;
        }
        //test tone variables
        double sinPos = 0;
        double sinPos2 = 0;

        private void ProcessSoundAfterCpuInstruction()
        {
            var startTime = systemTime.Now;

            if (cycleStartTime == null)
            {
                StartCycle(startTime);
                return;
            }

            TimeSpan ticks = (systemTime.Now - cycleStartTime).Value;


            if (  ticks.Ticks >= targetTicks  )
            {
                byte value = 0x00;
                bool value1 = (outputLatch.Value & SpeakerBit1) > 0;
                bool value2 = (outputLatch.Value & SpeakerBit2) > 0;

                if (value1 && !value2) value = 0xff;
                if (value2 && !value1) value = 0x00;

                var b = new byte[] { value };

                //test - generate continuous tones to test the buffering
                if (TestTone)
                {

                    double modulatedValue = Math.Sin(sinPos) * Math.Sin(sinPos2);
                    double amplitude = (modulatedValue + 1) * 128;
                    b[0] = (byte)((b[0]+(byte)amplitude)/2);

                    sinPos += 0.05;
                    sinPos2 += 0.11;
                    if (sinPos2 > Math.PI * 2)
                        sinPos2 = 0;
                    if (sinPos > Math.PI * 2)
                        sinPos = 0;
                }
                //end test code

                if (!SoundEnabled)
                    b[0] = 0x7F;

                buffer.Write(b, 0, 1);

                StartCycle(startTime);
            }
        }

        private void StartCycle(DateTime startTime)
        {
            cycleStartTime = startTime;
            instructionCount = 0;

        }

        private void LoopStream_StreamFinished(object sender, EventArgs e)
        {

            writer.Seek(0, SeekOrigin.Begin);
            //for (var x = 0; x < 16000; x++)
            //    writer.Write(0);

        }

        public class CircularBuffer<T>
        {
            private T[] buffer;
            private int readIndex;
            private int writeIndex;

            public CircularBuffer(int capacity)
            {
                buffer = new T[capacity];
                readIndex = 0;
                writeIndex = 0;
            }

            public int AvailableRead => writeIndex - readIndex;

            public int Capacity => buffer.Length;

            public void Write(T[] source, int offset, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[writeIndex % Capacity] = source[offset + i];
                    writeIndex++;
                }
            }

            public void Read(T[] destination, int offset, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    destination[offset + i] = buffer[readIndex % Capacity];
                    readIndex++;
                }
            }
        }
    }
}
