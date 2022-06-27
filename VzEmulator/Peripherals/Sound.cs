using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.MP3;
using CSCore.Codecs.RAW;
using CSCore.CoreAudioAPI;
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
        MemoryStream stream;

        const int SpeakerBit1 = 1; //1,32 = speaker. 2,4 = tape out
        const int SpeakerBit2 = 32; //1,32 = speaker. 2,4 = tape out

        const int samplesPerSecond = 16000;
        private const int Z80TargetKips = 1000; //540/4*3.5469

        DateTime? cycleStartTime;
        QueuedSoundPlayer player = new QueuedSoundPlayer();

        TimeSpan targetTicks;
        int instructionCount;
        int targetInstructionCount;
        private int delayTime = 2000;

        public bool SoundEnabled { get; set; } = false;

        public Sound(MemoryLatch outputLatch, ICpu cpu, SystemTime systemTime = null)
        {
            targetTicks = new TimeSpan(10000000 / samplesPerSecond); //10 million ticks in a second
            targetInstructionCount = Z80TargetKips * 1000 / samplesPerSecond; //instructions per sample
            //targetInstructionCount += 10;
            this.systemTime = systemTime ?? new SystemTimeDefaultImplementation();
            this.outputLatch = outputLatch;
            this.cpu = cpu;
            cpu.AfterInstructionExecution += Cpu_AfterInstructionExecution;

            stream = new MemoryStream(32000);
            writer = new BinaryWriter(stream);

            for (var x = 0; x < 16000; x++)
                writer.Write(0);

            IWaveSource soundSource = GetSoundSource(stream);
            LoopStream loopStream = new LoopStream(soundSource);
            loopStream.EnableLoop = true;
            loopStream.StreamFinished += LoopStream_StreamFinished;
            soundSource = loopStream;

            ISoundOut soundOut = GetSoundOut();
            soundOut.Initialize(soundSource);
            soundOut.Play();
        }

        private IWaveSource GetSoundSource(Stream stream)
        {
            // Instead of using the CodecFactory as helper, you specify the decoder directly:
            return new RawDataReader(stream, new WaveFormat(samplesPerSecond, 8, 1));

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

            if (instructionCount % 10 == 0)
                ProcessSoundAfterCpuInstruction();

        }

        private void ProcessSoundAfterCpuInstruction()
        {
            if (cycleStartTime == null)
            {
                StartCycle();
                return;
            }

            TimeSpan ticks = (systemTime.Now - cycleStartTime).Value;

            //for (int x = delayTime; x > 0; x--) ;

            if (ticks >= targetTicks)
            {
                if (instructionCount > targetInstructionCount)
                {
                    delayTime += 100;
                }
                else
                {
                    delayTime -= 100;
                    if (delayTime < 0)
                        delayTime = 0;
                }

                byte value = 0x7f;
                bool value1 = (outputLatch.Value & SpeakerBit1) > 0;
                bool value2 = (outputLatch.Value & SpeakerBit2) > 0;

                if (value1 && !value2) value = 0x0;
                if (value2 && !value1) value = 0xFF;

                writer.Write(value);

                StartCycle();
            }
        }

        private void StartCycle()
        {
            cycleStartTime = systemTime.Now;
            instructionCount = 0;
        }

        private void LoopStream_StreamFinished(object sender, EventArgs e)
        {
            writer.Seek(0, SeekOrigin.Begin);
            //for (var x = 0; x < 16000; x++)
            //    writer.Write(0);

        }
    }
}
